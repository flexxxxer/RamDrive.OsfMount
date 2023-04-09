using System;
using System.Linq;
using System.Threading.Tasks;

using ByteSizeLib;

using FluentAssertions;

using Xunit;

namespace RamDrive.OsfMount.IntegrationTests
{
  public class MountingWorkingTests : BoilerplateUsedNotUsedDriveLettersClass, IDisposable
  {
    public void Dispose()
    {
      foreach (var driveLetter in DriveLettersForUsage)
      {
        _ = OsfMountRamDrive.ForceUnmount(driveLetter).GetAwaiter().GetResult();
      }
    }

    [Fact]
    public async Task NtfsMountWorking()
    {
      var result = await OsfMountRamDrive.Mount(ByteSize.FromMebiBytes(300), DriveLettersForUsage.First(), FileSystemType.NTFS);

      result.IsT0.Should().BeFalse();
      result.IsT1.Should().BeTrue();
    }

    [Fact]
    public async Task Fat32MountWorking()
    {
      var result = await OsfMountRamDrive.Mount(ByteSize.FromMebiBytes(300), DriveLettersForUsage.Take(1).First(), FileSystemType.FAT32);

      result.IsT0.Should().BeFalse();
      result.IsT1.Should().BeTrue();
    }

    [Fact]
    public async Task ExFatMountWorking()
    {
      var result = await OsfMountRamDrive.Mount(ByteSize.FromMebiBytes(300), DriveLettersForUsage.Take(2).First(), FileSystemType.exFAT);

      result.IsT0.Should().BeFalse();
      result.IsT1.Should().BeTrue();
    }
  }
}
