using System;
using System.Linq;
using System.Threading.Tasks;

using ByteSizeLib;

using FluentAssertions;

using Xunit;

namespace RamDrive.OsfMount.IntegrationTests
{
  public class EnumerateAllMountedDrives : BoilerplateUsedNotUsedDriveLettersClass, IDisposable
  {
    public void Dispose()
    {
      foreach (var driveLetter in this.DriveLettersForUsage)
      {
        _ = OsfMountRamDrive.ForceUnmount(driveLetter).GetAwaiter().GetResult();
      }
    }

    [Fact]
    public async Task MountTwoAndCheckForListed()
    {
      var firstDriveLetter = this.DriveLettersForUsage.First();
      var firstDriveSize = ByteSize.FromMebiBytes(512);
      var firstDriveFs = FileSystemType.NTFS;
      var secondDriveLetter = this.DriveLettersForUsage.Skip(1).First();
      var secondDriveSize = ByteSize.FromMebiBytes(512);
      var secondDriveFs = FileSystemType.exFAT;

      await OsfMountRamDrive.Mount(firstDriveSize, firstDriveLetter, firstDriveFs);
      await OsfMountRamDrive.Mount(secondDriveSize, secondDriveLetter, secondDriveFs);

      var allMountedDrives = await OsfMountRamDrive.AllRamDrives().ToArrayAsync();

      var tenMb = ByteSize.FromMebiBytes(10);
      allMountedDrives.Should().HaveCount(2);
      allMountedDrives[0].DriveLetter.Should().Be(firstDriveLetter);
      allMountedDrives[0].FileSystem.Should().Be(firstDriveFs);
      allMountedDrives[0].Size.Should().BeInRange(firstDriveSize - tenMb, firstDriveSize + tenMb);
      allMountedDrives[1].DriveLetter.Should().Be(secondDriveLetter);
      allMountedDrives[1].FileSystem.Should().Be(secondDriveFs);
      allMountedDrives[1].Size.Should().BeInRange(secondDriveSize - tenMb, secondDriveSize + tenMb);
    }
  }
}