# How to contribute?
## Required development tools
* Visual Studio 2022 17.4 (or newer) or JetBrains Rider 2022.3 (or newer)
  * Or if you Hardcore Enjoyer, then Visual Studio Code
* .NET Framework 4.8 Developer pack
* .NET 6 SDK
* .NET 7 SDK
* PowerShell 7.3.0 or newer

## Building
Every time every time the RamDrive.OsfMount project is built, the script 
`Solution\RamDrive.OsfMount\ps-osfmount-validation\validate-osfmount-file-hashes.ps1` 
is run before it. This scripts validates OSFMount binary files: checks expected 
SHA512 hash with actual. If different, then raising error and build fails.
The scenario for running the script before assembly is described in `RamDrive.OsfMount.csproj`:
```xml
<Target Name="PreBuild" BeforeTargets="PreBuildEvent">
    <Exec Command="pwsh.exe -NonInteractive -File &quot;ps-osfmount-validation\validate-osfmount-file-hashes.ps1&quot;" />
</Target>
```
**You can delete this part if you build your own RamDrive.OsfMount library version and/or change
OSFMount binary files, but pull requests that remove the check script launch scenario or 
change the check script or change OSFMount binaries will not be considered and 
will be rejected**. Why? if it were possible, then the probability of substituting a malicious binary file would be high. 
**The only person who can change the OSFMount binaries or change the `validate-osfmount-file-hashes.ps1` 
script is the author of the repository**. but, it also remains possible for anyone to check the authenticity of the binary 
files in this repository through independent verification of the hashes of the files from the repository and the
original ones (the original ones can be obtained by installing OSFMount and going to its installation folder).
For example, using PowerShell function `Get-FileHash <file-path> -Algorithm <algorithm>`.

## Run tests
Because OSFMount requires admin rights, it requires the IDE or terminal to be
running with admin privileges. If you run `dotnet test` or some `run tests` command 
from the IDE not as an administrator, then almost all tests will fail.

## Issue before PullRequest
Just create an issue with a description of the functionality that you are missing 
(some api proposal, a semantic description of the functionality) BEFORE you implement 
it: it may not be within the responsibility of this library or it does not make sense, 
and it turns out that time is not worth wasting, maybe changes still won't be accepted.

## When can my pull request be rejected?
* When build or integration tests (`.github/test-on-push-or-pr.yml`) fail
* When changes harm the repository
  * Deleting documentation
  * Deleting or breaking functionality
  * Deleting or breaking github pipelines
* When deleting or replacing OSFMount's binary files
* When deleting or modifying OSFMount's binary verification script
* When removing any static analyzers or suppressing them in the 
amount of harmful code quality control (yes, everything is very 
subjective here, but the last word is up to the author of the repository)
* _there may be something else that the author of the repository did not guess at the time of writing this file..._

## What about code quality?
Just dont throw exceptions (pull requests with exception throwing will be rejected),
everything else is monitored by analyzers. Wrong formatting? Compilation error. 
Suppressing nullability without explanation? Compilation error. Are you not processing 
the result of a method or function call, and are you not using discard? Compilation error. 
Is the public method not documented? Compilation error. Struct constructor call with no parameters? 
Compilation error. Not all cases in switch statement handled? Compilation error. 
Yes, I can do this all day...