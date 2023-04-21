# CHANGELOG.md

## Some info about versioning
The library is versioned as follows:
[osfmount-binaries-version].[library-patch] where:
- osfmount-binaries-version - version of OSForensics OSFMount 
whose binaries are included in the release of the library
- library-patch - library patch number (bug fixes).

## 3.1.1001.4, 2023-04-21
New object-oriented API `RamDrive.OsfMount.ObjectOriented.RamDrive` and `OsfMountRamDrive.New` added.
API changed: returned type of `OsfMountRamDrive.Mount` was `MountError`, now `OneOf<MountError, Drive>`.

## 3.1.1001.3, 2023-04-8
New `OsfMountRamDrive.AllRamDrives` API added (fixed).

## 3.1.1001.2 (initial release), 2023-01-28
NOWWW package building is deterministic from CI-CD pipeline (fixed).

## 3.1.1001.1 (initial release), 2023-01-28
Readme added to nuget package. Now package building is deterministic from CI-CD pipeline.

## 3.1.1001.0 (initial release), 2023-01-23
Features:
- basic API implemented (Mount, Unmount, ForceUnmount)