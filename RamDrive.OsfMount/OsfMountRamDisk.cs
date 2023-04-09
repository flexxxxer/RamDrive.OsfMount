using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

using ByteSizeLib;

using EnumFastToStringGenerated;

using OneOf;

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

public record Drive(ByteSize Size, DriveLetter DriveLetter, FileSystemType FileSystem);

/// <summary>
/// Api for osfmount ram drive operations.
/// </summary>
public static class OsfMountRamDrive
{
  private static readonly ByteSize TotalRamCapacity = GetTotalRamCapacity();
  private static readonly string CliToolPath;
  private static readonly SemaphoreSlim Semaphore = new(1, 1);

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
  /// <returns><see cref="MountError"/> when some error, otherwise <see cref="Drive"/>.</returns>
  public static async Task<OneOf<MountError, Drive>> Mount(ByteSize size, DriveLetter? driveLetter, FileSystemType fileSystem)
  {
    await Semaphore.WaitAsync().ConfigureAwait(false);
    try
    {
      var sizeIsLowThenLimit = fileSystem switch
      {
        FileSystemType.NTFS => size.MebiBytes < 3,
        FileSystemType.exFAT => size.KibiBytes < 200,
        FileSystemType.FAT32 or _ => size.MebiBytes < 260,
      };

      if (sizeIsLowThenLimit)
      {
        return new MountError(new MountError.TooLowSize(size));
      }

      if (driveLetter is not null)
      {
        var letter = driveLetter.Value.ToStringFast()[0];

        // this check is not guaranteed to be free of problems, but it can probably detect a mount letter already in use
        if (DriveInfo.GetDrives().Any(d => d.Name.Length is 3 && d.Name[0] == letter))
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
      _ = process.Start();
      await process.WaitForExitAsync().ConfigureAwait(false);
      var stdOut = await process.StandardOutput.ReadToEndAsync().ConfigureAwait(false);
      var driveLetterRegex = new Regex(".*Created device \\d+: (.*): ->", RegexOptions.Compiled);

      // ! driveLetter!.Value will not throw exception if case will be
      return process.ExitCode switch
      {
        0 when driveLetterRegex.Matches(stdOut)
          .Cast<Match>()
          .FirstOrDefault()
          ?.Groups?[1]
          ?.Value
          ?.Pipe<DriveLetter>(Enum.TryParse)
          is DriveLetter letter => new Drive(size, letter, fileSystem),
        _ => new MountError(new MountError.DriveLetterInUseOrNotAllowed(driveLetter)),
      };
    }
    finally
    {
      _ = Semaphore.Release();
    }
  }

  /// <summary>
  /// Deletes/unmounts a specified by letter drive.
  /// </summary>
  /// <param name="driveLetter">Letter of drive to unmount.</param>
  /// <returns><see langword="null"/> if no errors, when some error then <see cref="UnmountError"/>.</returns>
  public static async Task<UnmountError?> Unmount(DriveLetter driveLetter)
  {
    await Semaphore.WaitAsync().ConfigureAwait(false);
    try
    {
      if (!await AllRamDrivesNoLock()
      .Select(d => d.DriveLetter)
      .ContainsAsync(driveLetter).ConfigureAwait(false))
      {
        return new UnmountError(new DriveDoesNotExistOrNotAllowed(driveLetter));
      }

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
      _ = process.Start();
      await process.WaitForExitAsync().ConfigureAwait(false);
      var stdError = await process.StandardError.ReadToEndAsync().ConfigureAwait(false);
      var stdOut = await process.StandardOutput.ReadToEndAsync().ConfigureAwait(false);
      return process.ExitCode switch
      {
        0 => null,
        not 0 when stdError.Contains("Access is denied") => new UnmountError(new UnmountError.DriveIsBusyWithAnotherProcess(driveLetter)),
        not 0 when stdOut.Contains("Done.") => null,
        not 0 => new UnmountError(new DriveDoesNotExistOrNotAllowed(driveLetter)),
      };
    }
    finally
    {
      _ = Semaphore.Release();
    }
  }

  /// <summary>
  /// Works like <see cref="Unmount(DriveLetter)"/>, but does not worry about other processes that use the specified drive.
  /// </summary>
  /// <param name="driveLetter">Letter of drive to unmount.</param>
  /// <returns><see langword="null"/> if no errors, otherwise <see cref="DriveDoesNotExistOrNotAllowed"/>.</returns>
  public static async Task<DriveDoesNotExistOrNotAllowed?> ForceUnmount(DriveLetter driveLetter)
  {
    await Semaphore.WaitAsync().ConfigureAwait(false);
    try
    {
      if (!await AllRamDrivesNoLock()
      .Select(d => d.DriveLetter)
      .ContainsAsync(driveLetter).ConfigureAwait(false))
      {
        return new DriveDoesNotExistOrNotAllowed(driveLetter);
      }

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
      _ = process.Start();
      await process.WaitForExitAsync().ConfigureAwait(false);
      return process.ExitCode switch
      {
        0 => null,
        not 0 => new DriveDoesNotExistOrNotAllowed(driveLetter),
      };
    }
    finally
    {
      _ = Semaphore.Release();
    }
  }

  /// <summary>
  /// Enumerates all active in-memory disks created via OsfMount.
  /// </summary>
  /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
  public static async IAsyncEnumerable<Drive> AllRamDrives()
  {
    await Semaphore.WaitAsync().ConfigureAwait(false);
    try
    {
      await foreach (var drive in AllRamDrivesNoLock().ConfigureAwait(false))
      {
        yield return drive;
      }
    }
    finally
    {
      _ = Semaphore.Release();
    }
  }

  private static async IAsyncEnumerable<Drive> AllRamDrivesNoLock()
  {
    foreach (var driveLetter in DriveLetterEnumExtensions.GetValuesFast())
    {
      var drive = await TryGetDriveFromLetter(driveLetter).ConfigureAwait(false);
      if (drive is not null)
      {
        yield return drive;
      }
    }

    static async Task<Drive?> TryGetDriveFromLetter(DriveLetter letter)
    {
      using var process = new Process
      {
        StartInfo = new ProcessStartInfo
        {
          UseShellExecute = false,
          RedirectStandardError = true,
          RedirectStandardOutput = true,
          FileName = CliToolPath,
          Arguments = $"-l -m {letter.ToStringFast()[0]}:",
          CreateNoWindow = true,
          WindowStyle = ProcessWindowStyle.Hidden,
        },
      };
      _ = process.Start();
      await process.WaitForExitAsync().ConfigureAwait(false);
      var stdOut = await process.StandardOutput.ReadToEndAsync().ConfigureAwait(false);
      return process.ExitCode switch
      {
        not 0 => null,
        0 when stdOut.Length is 0 => null,
        0 => DriveInfoExtract(letter),
      };
    }

    static Drive DriveInfoExtract(DriveLetter letter)
    {
      var driveInfo = DriveInfo
        .GetDrives()
        .Single(d => d.Name.StartsWith(letter.ToStringFast()));

      var fileSystem = driveInfo.DriveFormat.ToLower() switch
      {
        "ntfs" => FileSystemType.NTFS,
        "exfat" => FileSystemType.exFAT,
        "fat32" or _ => FileSystemType.FAT32,
      };

      return new(ByteSize.FromBytes(driveInfo.TotalSize), letter, fileSystem);
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
    _ = Directory.CreateDirectory(Path.Combine(tempDir, @"osfmount_bin\"));
    _ = Directory.CreateDirectory(Path.Combine(tempDir, @"osfmount_bin\win10\"));

    // for "CWE-23 Relative Path Traversal"
    var manifestResourceNames = assembly.GetManifestResourceNames();
    if (manifestResourceNames.Any(rn => rn.Contains(new string(new[] { '.', '.' }))))
    {
      throw new SecurityException($"Assembly {assembly.FullName} was corrupted.");
    }

    foreach (var resourceName in manifestResourceNames)
    {
      var resourcePath = resourceName
        .Replace(resourcePrefix, string.Empty)
        .Replace("osfmount_bin.win10.", @"osfmount_bin\win10\")
        .Replace("osfmount_bin.", @"osfmount_bin\");

      using var resourceStream = assembly.GetManifestResourceStream(resourceName);
      using var fileStream = new FileStream(Path.Combine(tempDir, resourcePath), FileMode.OpenOrCreate, FileAccess.Write, FileShare.Write);
      resourceStream?.CopyTo(fileStream);
    }

    return (tempDir, Path.Combine(tempDir, @"osfmount_bin\OSFMount.com"));
  }
}
