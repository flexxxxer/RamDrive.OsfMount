using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using ByteSizeLib;

using FluentAssertions;

using Xunit;

namespace RamDrive.OsfMount.IntegrationTests
{
  public class UnmountNotWorkingWhenDiskUsedBySomeone : BoilerplateUsedNotUsedDriveLettersClass, IDisposable
  {
    public void Dispose()
    {
      foreach (var driveLetter in DriveLettersForUsage)
      {
        _ = OsfMountRamDrive.ForceUnmount(driveLetter).GetAwaiter().GetResult();
      }
    }

    [Fact]
    public async Task UnmountNotWorkingWhenWritingFile()
    {
      var driveLetter = DriveLettersForUsage.First();

      var mountResult = await OsfMountRamDrive.Mount(ByteSize.FromMebiBytes(500), driveLetter, FileSystemType.NTFS);
      mountResult.Should().BeNull();

      var fileStream = File.Create($"{driveLetter}:/testfile.txt");
      var steamWriter = new StreamWriter(fileStream);

      steamWriter.WriteLine("Hello, World!");
      var unmountResult = await OsfMountRamDrive.Unmount(driveLetter);
      unmountResult.Should().NotBeNull();

      // ReSharper disable once PossibleNullReferenceException
      // suppression not works, set code level analysis to C#8 breaks build.
      unmountResult.IsT0.Should().BeTrue();

      steamWriter.Dispose();
      fileStream.Dispose();

      // waiting for the OS to double up and release the descriptors
      // and all sorts of other buzzwords
      await Task.Delay(TimeSpan.FromSeconds(10));

      unmountResult = await OsfMountRamDrive.Unmount(driveLetter);
      unmountResult.Should().BeNull();
    }
  }
}
