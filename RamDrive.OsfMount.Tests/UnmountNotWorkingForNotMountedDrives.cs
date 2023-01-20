using System.Linq;
using System.Threading.Tasks;

using FluentAssertions;

using Xunit;

namespace RamDrive.OsfMount.IntegrationTests
{
  public class UnmountNotWorkingForNotMountedDrives : BoilerplateUsedNotUsedDriveLettersClass
  {
    [Fact]
    public async Task UnmountOnNotUsedDrivesNotWorking()
    {
      var resultsCollection = await DriveLettersForUsage.ToAsyncEnumerable()
        .SelectAwait(async l => await OsfMountRamDrive.Unmount(l))
        .ToArrayAsync();

      resultsCollection.Should().NotContainNulls();
    }
  }
}

