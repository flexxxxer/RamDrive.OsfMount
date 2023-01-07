// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

// There is a place for constantly used suppressions, above are only temporary
[assembly: SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1117:Parameters should be on same line or separate lines",
    Justification = "Supression lines too long")]
[assembly: SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1633:File should have header",
    Justification = "I want the file system names to be original")]
[assembly: SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:Element should begin with upper-case letter",
    Justification = "I want the file system names to be original", Scope = "member", Target = "~F:RamDrive.OsfMount.FileSystemType.exFAT")]
[assembly: SuppressMessage("StyleCop.CSharp.OrderingRules", "SA1200:Using directives should be placed correctly",
    Justification = "It's boring and useless")]
[assembly: SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1136:Enum values should be on separate lines",
    Justification = "26 lines for just letters? No", Scope = "type", Target = "~T:RamDrive.OsfMount.DriveLetter")]
[assembly: SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1602:Enumeration items should be documented",
    Justification = "Just don't want to write boilerplate comments to filesystem types", Scope = "type", Target = "~T:RamDrive.OsfMount.FileSystemType")]
[assembly: SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1602:Enumeration items should be documented",
    Justification = "There are only unique letters, I think everyone knows the alphabet", Scope = "type", Target = "~T:RamDrive.OsfMount.DriveLetter")]
[assembly: SuppressMessage("StyleCop.CSharp.LayoutRules", "SA1516:Elements should be separated by blank line",
    Justification = "Just dont want to separate type definitions", Scope = "type", Target = "~T:RamDrive.OsfMount.MountError.DriveSizeCannotBeGreaterThenTotalRamCapacity")]
[assembly: SuppressMessage("StyleCop.CSharp.LayoutRules", "SA1516:Elements should be separated by blank line",
    Justification = "Just don't want to separate type definitions", Scope = "type", Target = "~T:RamDrive.OsfMount.MountError.TooLowSize")]
[assembly: SuppressMessage("StyleCop.CSharp.NamingRules", "SA1313:Parameter names should begin with lower-case letter",
    Justification = "StyleCop don't understand C# 9 record-types", Scope = "type", Target = "~T:RamDrive.OsfMount.MountError.DriveLetterInUseOrNotAllowed")]
[assembly: SuppressMessage("StyleCop.CSharp.NamingRules", "SA1313:Parameter names should begin with lower-case letter",
    Justification = "StyleCop don't understand C# 9 record-types", Scope = "type", Target = "~T:RamDrive.OsfMount.MountError.TooLowSize")]
[assembly: SuppressMessage("StyleCop.CSharp.NamingRules", "SA1313:Parameter names should begin with lower-case letter",
    Justification = "StyleCop don't understand C# 9 record-types", Scope = "type", Target = "~T:RamDrive.OsfMount.MountError.DriveSizeCannotBeGreaterThenTotalRamCapacity")]
[assembly: SuppressMessage("StyleCop.CSharp.NamingRules", "SA1313:Parameter names should begin with lower-case letter",
    Justification = "StyleCop don't understand C# 9 record-types", Scope = "type", Target = "~T:RamDrive.OsfMount.UnmountError.DriveIsBusyWithAnotherProcess")]
[assembly: SuppressMessage("StyleCop.CSharp.NamingRules", "SA1313:Parameter names should begin with lower-case letter",
    Justification = "StyleCop don't understand C# 9 record-types", Scope = "type", Target = "~T:RamDrive.OsfMount.DriveDoesNotExistOrNotAllowed")]
