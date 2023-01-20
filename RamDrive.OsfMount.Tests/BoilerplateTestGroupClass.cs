using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace RamDrive.OsfMount.IntegrationTests
{
  public abstract class BoilerplateUsedNotUsedDriveLettersClass
  {
    protected ReadOnlyCollection<DriveLetter> DriveLettersForUsage { get; }
    protected ReadOnlyCollection<DriveLetter> DriveLettersWhichUsedByUser { get; }

    protected BoilerplateUsedNotUsedDriveLettersClass()
    {
      var activeUsedDriveLetters = DriveInfo.GetDrives()
        .Where(d => d.Name.Length is 3)
        .Select(d => d.Name[0].ToString())
        .Where(d => Enum.TryParse(d, out DriveLetter _))
        .Select(d => (DriveLetter)Enum.Parse(typeof(DriveLetter), d))
        .ToArray();

      var allPossibleDriveLetters = (DriveLetter[])Enum.GetValues(typeof(DriveLetter));

      var notUsedDriveLetters = allPossibleDriveLetters
        .Except(activeUsedDriveLetters)
        .ToArray();

      DriveLettersForUsage = new ReadOnlyCollection<DriveLetter>(notUsedDriveLetters);
      DriveLettersWhichUsedByUser = new ReadOnlyCollection<DriveLetter>(activeUsedDriveLetters);
    }
  }
}
