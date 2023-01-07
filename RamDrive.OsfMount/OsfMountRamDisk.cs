﻿using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using ByteSizeLib;
using EnumFastToStringGenerated;

#if NETFRAMEWORK
using Hardware.Info;
#endif

namespace RamDrive.OsfMount;

/// <summary>
/// Type of file system in which the ram drive will be formatted.
/// </summary>
[EnumGenerator]
public enum FileSystemType
{
    // ReSharper disable once InconsistentNaming
    NTFS,

    // ReSharper disable once InconsistentNaming
    FAT32,

    // ReSharper disable once InconsistentNaming
    exFAT,
}

/// <summary>
/// Letter to which the ram drive will be assigned.
/// </summary>
[EnumGenerator]
public enum DriveLetter
{
    // A, B excluded because causes not working unmount while A or B drive used by osfmount.
    // C excluded because just system.
    D, E, F, G, H,
    I, J, K, L, M,
    N, O, P, Q, R,
    S, T, U, V, W,
    X, Y, Z,
}

/// <summary>
/// Api for osfmount ram drive operations.
/// </summary>
public static class OsfMountRamDrive
{
    private static readonly ByteSize TotalRamCapacity = GetTotalRamCapacity();
    private static readonly string CliToolPath;
    private static readonly SemaphoreSlim Semaphore = new (1, 1);

    static OsfMountRamDrive()
    {
        (var embeddedResourcesPath, CliToolPath) = LoadEmbeddedResources();

        AppDomain.CurrentDomain.ProcessExit += (_, _) =>
        {
            try
            {
                Directory.Delete(embeddedResourcesPath);
            }
            catch
            {
                // don't care
            }
        };
    }

    /// <summary>
    /// Creates/mounts a ram drive, assigns a drive letter, and formats to the specified file system.
    /// </summary>
    /// <param name="size">Size of ram drive.</param>
    /// <param name="driveLetter">Letter of ram drive. If <see langword="null"/>, then the first available will be will be assigned.</param>
    /// <param name="fileSystem">File system of ram drive.</param>
    /// <returns><see langword="null"/> if no errors, when some error then <see cref="MountError"/>.</returns>
    public static async Task<MountError?> MountAsync(ByteSize size, DriveLetter? driveLetter, FileSystemType fileSystem)
    {
        await Semaphore.WaitAsync();
        try
        {
            var sizeIsLowThenLimit = fileSystem switch
            {
                FileSystemType.FAT32 => size.MebiBytes < 260,
                FileSystemType.NTFS => size.MebiBytes < 3,
                FileSystemType.exFAT => size.KibiBytes < 200,
                _ => throw new NotImplementedException(),
            };

            if (sizeIsLowThenLimit)
            {
                return new MountError(new MountError.TooLowSize(size));
            }

            if (driveLetter is not null)
            {
                var letter = driveLetter.Value.ToStringFast()[0];

                // this check is not guaranteed to be free of problems, but it can probably detect a mount letter already in use
                if (DriveInfo.GetDrives().Where(d => d.Name.Length is 3).Any(d => d.Name[0] == letter))
                {
                    return new MountError(new MountError.DriveLetterInUseOrNotAllowed(driveLetter.Value));
                }
            }

            // I consider the limitation in the form of a reserve of 1 gigabyte for the operating system to be solid
            if (TotalRamCapacity - ByteSize.FromGigaBytes(1) <= size)
            {
                return new MountError(new MountError.DriveSizeCannotBeGreaterThenTotalRamCapacity(size));
            }

            // I consider the limitation in the form of a reserve of 1 gigabyte for the operating system to be solid
            if (TotalRamCapacity - ByteSize.FromGigaBytes(1) <= size)
            {
                return new MountError(new MountError.DriveSizeCannotBeGreaterThenTotalRamCapacity(size));
            }

            var driveLetterSymbol = driveLetter switch
            {
                // from osfmount documentation:
                // "When creating a new logical disk you can specify "-m #:" as mountpoint
                // in which case the first unused drive letter is automatically used"
                null => '#',
                { } notNull => notNull.ToStringFast()[0],
            };

            // osfmount taking not bytes count, need
            // to pass just count of blocks with size 512 bytes. idk why, and don't care
            var sizeInBytesButDiv512 = (long)size.Bytes / 512;
            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    UseShellExecute = false,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    FileName = CliToolPath,
                    Arguments = $"-a -t vm -o format:{fileSystem.ToStringFast().ToLower()},logical -m {driveLetterSymbol}: -s {sizeInBytesButDiv512}b",
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                },
            };
            process.Start();
            await process.WaitForExitAsync();
            return process.ExitCode switch
            {
                0 => null,
                not 0 => new MountError(new MountError.DriveLetterInUseOrNotAllowed(driveLetter)),
            };
        }
        finally
        {
            Semaphore.Release();
        }
    }

    /// <summary>
    /// Deletes/unmounts a specified by letter drive.
    /// </summary>
    /// <param name="driveLetter">Letter of drive to unmount.</param>
    /// <returns><see langword="null"/> if no errors, when some error then <see cref="UnmountError"/>.</returns>
    public static async Task<UnmountError?> UnmountAsync(DriveLetter driveLetter)
    {
        await Semaphore.WaitAsync();
        try
        {
            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    UseShellExecute = false,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    FileName = CliToolPath,
                    Arguments = $"-d -m {driveLetter.ToStringFast()}:",
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                },
            };
            process.Start();
            await process.WaitForExitAsync();
            return process.ExitCode switch
            {
                0 => null,
                not 0 when process.StandardError.ReadToEnd().Contains("Access is denied") => new UnmountError(new UnmountError.DriveIsBusyWithAnotherProcess(driveLetter)),
                not 0 => new UnmountError(new DriveDoesNotExistOrNotAllowed(driveLetter)),
            };
        }
        finally
        {
            Semaphore.Release();
        }
    }

    /// <summary>
    /// Works like <see cref="UnmountAsync(DriveLetter)"/>, but does not worry about other processes that use the specified drive.
    /// </summary>
    /// <param name="driveLetter">Letter of drive to unmount.</param>
    /// <returns><see langword="null"/> if no errors, otherwise <see cref="DriveDoesNotExistOrNotAllowed"/>.</returns>
    public static async Task<DriveDoesNotExistOrNotAllowed?> ForceUnmountAsync(DriveLetter driveLetter)
    {
        await Semaphore.WaitAsync();
        try
        {
            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    UseShellExecute = false,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    FileName = CliToolPath,
                    Arguments = $"-D -m {driveLetter.ToStringFast()}:",
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                },
            };
            process.Start();
            await process.WaitForExitAsync();
            return process.ExitCode switch
            {
                0 => null,
                not 0 => new DriveDoesNotExistOrNotAllowed(driveLetter),
            };
        }
        finally
        {
            Semaphore.Release();
        }
    }

    private static ByteSize GetTotalRamCapacity()
    {
#if NET6_0_OR_GREATER
        return ByteSize.FromBytes(GC.GetGCMemoryInfo().TotalAvailableMemoryBytes);
#else
        var hwInfo = new HardwareInfo();
        hwInfo.RefreshMemoryStatus();
        return ByteSize.FromBytes(hwInfo.MemoryStatus.TotalPhysical);
#endif
    }

    private static (string EmbeddedReourcesPath, string CliToolPath) LoadEmbeddedResources()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName()) + @"\";
        var resourcePrefix = Assembly.GetExecutingAssembly().GetName().Name + ".";

        _ = Directory.CreateDirectory(tempDir);
        _ = Directory.CreateDirectory(tempDir + @"osfmount_bin\");
        _ = Directory.CreateDirectory(tempDir + @"\osfmount_bin\win10\");
        foreach (var resourceName in assembly.GetManifestResourceNames())
        {
            var resourcePath = resourceName
                    .Replace(resourcePrefix, string.Empty)
                    .Replace("osfmount_bin.win10.", @"osfmount_bin\win10\")
                    .Replace("osfmount_bin.", @"osfmount_bin\");

            using var resourceStream = assembly.GetManifestResourceStream(resourceName);
            using var fileStream = new FileStream(tempDir + resourcePath, FileMode.OpenOrCreate, FileAccess.Write);
            resourceStream?.CopyTo(fileStream);
        }

        return (tempDir, tempDir + @"osfmount_bin\OSFMount.com");
    }

#if NETFRAMEWORK
    private static async Task WaitForExitAsync(this Process process, CancellationToken cancellationToken = default)
    {
        var tcs = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);

        void ProcessExited(object? sender, EventArgs e)
        {
            tcs.TrySetResult(process.ExitCode);
        }

        try
        {
            process.EnableRaisingEvents = true;
        }
        catch (InvalidOperationException) when (process.HasExited)
        {
            // ignoring, don't care
        }

        using (cancellationToken.Register(() => tcs.TrySetCanceled()))
        {
            process.Exited += ProcessExited;

            try
            {
                if (process.HasExited)
                {
                    tcs.TrySetResult(null!);
                }

                await tcs.Task.ConfigureAwait(false);
            }
            finally
            {
                process.Exited -= ProcessExited;
            }
        }
    }
#endif
}