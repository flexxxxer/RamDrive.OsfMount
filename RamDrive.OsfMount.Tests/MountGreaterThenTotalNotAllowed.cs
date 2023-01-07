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
            var mountResult = await OsfMountRamDrive.MountAsync(
                ByteSize.FromTebiBytes(1024),
                null,
                FileSystemType.NTFS);

            mountResult.Should().NotBeNull();

            // ReSharper disable once PossibleNullReferenceException
            // suppression not works, set code level analysis to C#8 breaks build.
            mountResult.IsT2.Should().BeTrue();
        }

        // Maybe the first one will stop working, then this one should work
        [Fact]
        public async Task MountMaxAvailablePlus300MbFailed()
        {
            // ReSharper disable once PossibleNullReferenceException
            // just annoying
            var totalAvailableNow = (ByteSize) typeof(OsfMountRamDrive)
                .GetMethod("GetTotalRamCapacity", BindingFlags.Static | BindingFlags.NonPublic)
                .Invoke(null, null);

            var mountResult = await OsfMountRamDrive.MountAsync(
                totalAvailableNow + ByteSize.FromMebiBytes(300),
                null,
                FileSystemType.NTFS);

            mountResult.Should().NotBeNull();

            // ReSharper disable once PossibleNullReferenceException
            // suppression not works, set code level analysis to C#8 breaks build.
            mountResult.IsT2.Should().BeTrue();
        }
    }
}
