# RamDrive.OsfMount
**RamDrive.OsfMount** is a wrapper library for 
[OSForensics OSFMount](https://www.osforensics.com/tools/mount-disk-images.html) 
that allows you to create and manage disks mounted in RAM (no other features provided by design).
Supports Windows 11, 10, 8 (theoretical, not tested), 7 SP1 (theoretical, not tested), 
_only 64 bit_ (there is OSFMount v2 for 32-bit support, but this library does not support
this outdated version).

# Get Started
## Requirements
Library targeting:
* .NET Framework (4.6.2, 4.7, 4.7.1, 4.7.2, 4.8)
* .NET (6-windows-only, 7-windows-only)

If the target platform of the project in which you are going to use this library is 
not Windows, then there is no point in using it. In the case of a cross-platform project, 
you will need to use the 
[Conditional PackageReference](https://learn.microsoft.com/en-us/nuget/consume-packages/package-references-in-project-files#adding-a-packagereference-condition),
where for Windows will be used this library.
## Install Package
Using Package Manager Console:
```shell
PM> Install-Package RamDrive.OsfMount
```
Using .NET CLI:
```shell
dotnet add package RamDrive.OsfMount
```
## Usage
**Please note that all operations require your application to have administrator privileges**.

Mount ram drive with size 1.5Gb, under drive letter 'X' and file system NTFS:
```csharp
using RamDrive.OsfMount;
using ByteSizeLib;

var possibleError = await OsfMountRamDrive.MountAsync(
    ByteSize.FromGibiBytes(1.5),
    DriveLetter.X,
    FileSystemType.NTFS
);
```

Unmount ram drive under drive letter 'X':
```csharp
using RamDrive.OsfMount;
using ByteSizeLib;

var possibleError = await OsfMountRamDrive.UnmountAsync(DriveLetter.X);
```

Force unmount ram drive under drive letter 'X' (force unmount will be abort all drive
under letter 'X' operations from other processes in system if exists):
```csharp
using RamDrive.OsfMount;
using ByteSizeLib;

var possibleError = await OsfMountRamDrive.ForceUnmountAsync(DriveLetter.X);
```

How to handle errors? Type of returned objects — types of errors described using 
discriminated union ([OneOf type library](https://github.com/mcintyre321/OneOf/)),
which have `Switch` and `Match` methods (for pattern matching). See:

```csharp
using RamDrive.OsfMount;
using ByteSizeLib;
using OneOf;

var result = await OsfMountRamDrive.MountAsync(
     ByteSize.FromMebiBytes(300),
     DriveLettersForUsage.First(),
     FileSystemType.NTFS);
 
// Log if some error
result?.Switch(
    driveLetterInUseOrNotAllowed => Logger.LogDriveLetterIllegal(driveLetterInUseOrNotAllowed.DriveLetter),
    tooLowSize => Logger.TooLowSizeForDrive(tooLowSize.Size),
    driveSizeCannotBeGreaterThenRamCapacity => Logger.TooBigDriveSize(driveSizeCannotBeGreaterThenRamCapacity.Size)
);

// Or create error message or nothing if some error
string? message = result?.Match(
    case1 => $"Drive letter {case1.DriveLetter} in use or not allowed",
    case2 => $"Drive size {case2.Size} too low",
    case3 => "Drive size cannot be greater then total ram capacity"
);
```

# Contributing
See [CONTRIBUTION.md](https://github.com/flexxxxer/RamDrive.OsfMount/blob/master/CONTRIBUTION.md).

# License
RamDisk is Copyright © 2023 Copyright © 2023 flexxxxer Aleksandr under the [Apache License, Version 2.0](https://github.com/flexxxxer/RamDrive.OsfMount/blob/master/License.txt).