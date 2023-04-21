using System;
using System.Threading;
using System.Threading.Tasks;

using ByteSizeLib;

using EnumFastToStringGenerated;

namespace RamDrive.OsfMount.ObjectOriented;

/// <summary>
/// Disposiable wrapper for <see cref="OsfMountRamDrive"/> API.
/// </summary>
public sealed class RamDrive : IDisposable, IAsyncDisposable
{
  private readonly DriveLetter driveLetter;
  private readonly ByteSize size;
  private readonly FileSystemType fileSystem;
  private int disposed;

  /// <summary>
  /// Gets drive letter.
  /// </summary>
  public DriveLetter DriveLetter
  {
    get
    {
      this.CheckDisposed();
      return this.driveLetter;
    }
  }

  /// <summary>
  /// Gets drive size.
  /// </summary>
  public ByteSize Size
  {
    get
    {
      this.CheckDisposed();
      return this.size;
    }
  }

  /// <summary>
  /// Gets drive filesystem.
  /// </summary>
  public FileSystemType FileSystem
  {
    get
    {
      this.CheckDisposed();
      return this.fileSystem;
    }
  }

  /// <summary>
  /// Gets path to drive. For drive with letter <see cref="OsfMount.DriveLetter.H"/> will be <c>"H:"</c>.
  /// </summary>
  public string Path
  {
    get
    {
      this.CheckDisposed();
      return @$"{this.DriveLetter.ToStringFast()}:\";
    }
  }

  private RamDrive(ByteSize size, DriveLetter letter, FileSystemType fileSystem)
  {
    this.driveLetter = letter;
    this.size = size;
    this.fileSystem = fileSystem;
    this.disposed = 0;
  }

  private RamDrive(Drive drive)
    : this(drive.Size, drive.DriveLetter, drive.FileSystem)
  {
  }

  /// <summary>
  /// Creates and mounts new ram drive with first free drive letter.
  /// </summary>
  /// <param name="size">Drive size.</param>
  /// <param name="fileSystem">Drive filesystem.</param>
  /// <returns>New <see cref="RamDrive"/> instance.</returns>
  public static async Task<RamDrive> New(ByteSize size, FileSystemType fileSystem) => await New(size, fileSystem, null);

  /// <summary>
  /// Creates and mounts new ram drive.
  /// </summary>
  /// <param name="size">Drive size.</param>
  /// <param name="fileSystem">Drive filesystem.</param>
  /// <param name="driveLetter">Drive letter. If null, then will be assigned first free letter.</param>
  /// <returns>New <see cref="RamDrive"/> instance.</returns>
  /// <exception cref="DriveLetterInUseOrNotAllowedException">When letter is in use or no free drive letters.</exception>
  /// <exception cref="TooLowSizeException">When size too low.</exception>
  /// <exception cref="TooBigSizeException">When size bigger then total ram capacity.</exception>
  [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.SpacingRules", "SA1009:Closing parenthesis should be spaced correctly", Justification = "So readable.")]
  [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1111:Closing parenthesis should be on line of last parameter", Justification = "So readable.")]
  public static async Task<RamDrive> New(ByteSize size, FileSystemType fileSystem, DriveLetter? driveLetter)
  {
    var mountResult = await OsfMountRamDrive.Mount(size, driveLetter, fileSystem);
    if (mountResult.TryPickT0(out var error, out var newDrive))
    {
      var exception = error.Match<Exception>(
        driveLetterError => new DriveLetterInUseOrNotAllowedException(driveLetterError.DriveLetter),
        tooLowSizeError => new TooLowSizeException(tooLowSizeError.Size),
        sizeError => new TooBigSizeException(sizeError.Size)
      );

      throw exception;
    }

    return new RamDrive(newDrive);
  }

  /// <summary>
  /// Unmounts ram drive.
  /// </summary>
  [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.LayoutRules", "SA1501:Statement should not be on a single line", Justification = "ThreadAbortException")]
  [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE2001:Embedded statements must be on their own line", Justification = "ThreadAbortException")]
  [System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S108:Nested blocks of code should not be left empty", Justification = "ThreadAbortException")]
  void IDisposable.Dispose()
  {
    try { }
    finally
    {
      if (Interlocked.CompareExchange(ref this.disposed, 1, 0) == 0)
      {
        _ = OsfMountRamDrive.ForceUnmount(this.driveLetter).GetAwaiter().GetResult();
      }
    }
  }

  /// <summary>
  /// Unmounts ram drive.
  /// </summary>
  /// <returns>A <see cref="ValueTask"/> representing the result of the asynchronous operation.</returns>
  [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.LayoutRules", "SA1501:Statement should not be on a single line", Justification = "ThreadAbortException")]
  [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE2001:Embedded statements must be on their own line", Justification = "ThreadAbortException")]
  [System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S108:Nested blocks of code should not be left empty", Justification = "ThreadAbortException")]
  async ValueTask IAsyncDisposable.DisposeAsync()
  {
    try { }
    finally
    {
      if (Interlocked.CompareExchange(ref this.disposed, 1, 0) == 0)
      {
        _ = await OsfMountRamDrive.ForceUnmount(this.driveLetter);
      }
    }
  }

  private void CheckDisposed()
  {
    if (this.disposed == 1)
    {
      throw new ObjectDisposedException(nameof(RamDrive));
    }
  }
}
