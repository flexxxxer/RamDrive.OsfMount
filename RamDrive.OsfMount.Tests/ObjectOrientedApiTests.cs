using System.IO;
using System.Text;
using System.Threading.Tasks;

using ByteSizeLib;

using FluentAssertions;

using Xunit;

namespace RamDrive.OsfMount.IntegrationTests
{
  public class ObjectOrientedApiTests
  {
    [Fact]
    public async Task WriteFileReadFile()
    {
      const string fileName = "text.txt";
      const string text = "venire contra factum proprium";
      using (var drive = await OsfMountRamDrive.New(ByteSize.FromMebiBytes(512), FileSystemType.NTFS))
      {
        File.WriteAllText(Path.Combine(drive.Path, fileName), text);
        File.ReadAllText(Path.Combine(drive.Path, fileName)).Should().BeEquivalentTo(text);
      }
    }
  }
}
