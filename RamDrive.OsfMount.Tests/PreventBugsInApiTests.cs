using System;
using System.Linq;

using ByteSizeLib;

using FluentAssertions;

using Xunit;

namespace RamDrive.OsfMount.IntegrationTests
{
  public class PreventBugsInApiTests
  {
    [Fact]
    public void OsfMountRamDiskMountAsyncNotThrowsOnAnyOfFilesystemEnumValues()
    {
      Record.Exception(() =>
      {
        foreach (var fileSystem in Enum.GetValues(typeof(FileSystemType)).Cast<FileSystemType>())
        {
          // must be error (MountError.TooLowSize) but not exception
          _ = OsfMountRamDrive.Mount(ByteSize.FromBytes(1), null, fileSystem).GetAwaiter().GetResult();
        }

      }).Should().BeNull();
    }
  }
}
