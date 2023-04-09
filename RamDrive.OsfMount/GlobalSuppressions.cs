// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

// There is a place for constantly used suppressions, above are only temporary
[assembly: SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1117:Parameters should be on same line or separate lines",
  Justification = "Supression lines too long")]
[assembly: SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:File may only contain a single type",
  Justification = "Just no")]
[assembly: SuppressMessage("StyleCop.CSharp.OrderingRules", "SA1201:A record struct should not follow a class",
  Justification = "Just no")]
[assembly: SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1633:File should have header",
  Justification = "Just no")]
[assembly: SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1118:Parameter should not span multiple lines",
  Justification = "Just no")]
[assembly: SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1649:File name should match first type name",
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
[assembly: SuppressMessage("Puma.Security.Rules", "SEC0032:Untrusted data is passed to the ProcessStartInfo fileName or arguments parameter",
  Justification = "There is no way to send an execution command in any form other than in direct text form.", Scope = "member",
  Target = "~M:RamDrive.OsfMount.OsfMountRamDrive.Mount(ByteSizeLib.ByteSize,System.Nullable{RamDrive.OsfMount.DriveLetter},RamDrive.OsfMount.FileSystemType)")]
[assembly: SuppressMessage("Puma.Security.Rules", "SEC0032:Untrusted data is passed to the ProcessStartInfo fileName or arguments parameter",
  Justification = "There is no way to send an execution command in any form other than in direct text form.", Scope = "member",
  Target = "~M:RamDrive.OsfMount.OsfMountRamDrive.Unmount(RamDrive.OsfMount.DriveLetter)")]
[assembly: SuppressMessage("Puma.Security.Rules", "SEC0032:Untrusted data is passed to the ProcessStartInfo fileName or arguments parameter",
  Justification = "There is no way to send an execution command in any form other than in direct text form.", Scope = "member",
  Target = "~M:RamDrive.OsfMount.OsfMountRamDrive.ForceUnmount(RamDrive.OsfMount.DriveLetter)")]
[assembly: SuppressMessage("Puma.Security.Rules", "SEC0112:Unvalidated file paths are passed to a FileStream API, which can allow unauthorized file system operations (e.g. read, write, delete) to be performed on unintended server files.",
  Justification = """
  The path in this section of code is generated: the concatenation of the path of the temporary directory obtained via 
  Path.GetTempPath and the name of the resource. If the name of the resource differs from the expected ones (they are 
  declared in OsfMountRamDrive.ExpectedManifestResources), that is, store, for example, "../", then 
  the library will consider the resource manifest and the resources themselves compromised. 
  There is protection against CWE-23, but not in the form that a static analyzer wants to see.
  """, Scope = "member", Target = "~M:RamDrive.OsfMount.OsfMountRamDrive.LoadEmbeddedResources()")]
[assembly: SuppressMessage("Security", "SEC0032:Command Injection Process Start Info", Scope = "member",
  Target = "~M:RamDrive.OsfMount.OsfMountRamDrive.AllRamDrivesNoLock~System.Collections.Generic.IAsyncEnumerable{RamDrive.OsfMount.Drive}",
  Justification = "No way to pass command in another way.")]
[assembly: SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:Elements should be documented", Scope = "type",
  Target = "~T:RamDrive.OsfMount.Extensions.TryParse`1",
  Justification = "Not necessary")]
[assembly: SuppressMessage("Style", "IDE0022:Use block body for method", Scope = "member",
  Target = "~M:RamDrive.OsfMount.Extensions.Pipe``1(System.String,RamDrive.OsfMount.Extensions.TryParse{``0})~``0",
  Justification = "Not necessary")]
[assembly: SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:Elements should be documented", Scope = "member",
  Target = "~M:RamDrive.OsfMount.Extensions.Pipe``1(System.String,RamDrive.OsfMount.Extensions.TryParse{``0})~``0",
  Justification = "Not necessary")]
