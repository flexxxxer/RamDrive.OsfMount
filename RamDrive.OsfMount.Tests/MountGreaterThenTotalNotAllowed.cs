using System.Reflection;
using System.Threading.Tasks;

using ByteSizeLib;

using FluentAssertions;

using Xunit;

namespace RamDrive.OsfMount.IntegrationTests
{
  public class MountGreaterThenTotalNotAllowed
  {
    [Fact]
    public async Task Mount1024TbFailed()
    {
      var mountResult = await OsfMountRamDrive.Mount(ByteSize.FromTebiBytes(1024), null, FileSystemType.NTFS);

      mountResult.IsT0.Should().BeTrue();
      mountResult.AsT0.IsT2.Should().BeTrue();
    }

    // Maybe the first one will stop working, then this one should work
    [Fact]
    public async Task MountMaxAvailablePlus300MbFailed()
    {
      // ReSharper disable once PossibleNullReferenceException
      // just annoying
      var totalAvailableNow = (ByteSize)typeof(OsfMountRamDrive)
          .GetMethod("GetTotalRamCapacity", BindingFlags.Static | BindingFlags.NonPublic)
          .Invoke(null, null);

      var mountResult = await OsfMountRamDrive.Mount(
          totalAvailableNow + ByteSize.FromMebiBytes(300),
          null,
          FileSystemType.NTFS);

      mountResult.IsT0.Should().BeTrue();
      mountResult.AsT0.IsT2.Should().BeTrue();
    }
  }
}
