using System;

using ByteSizeLib;

namespace RamDrive.OsfMount.ObjectOriented;
#pragma warning disable RCS1194 // Implement exception constructors.
#pragma warning disable S3925 // "ISerializable" should be implemented correctly
#pragma warning disable SA1600 // Elements should be documented

public class DriveLetterInUseOrNotAllowedException : Exception
{
  public DriveLetter? DriveLetter { get; }

  public DriveLetterInUseOrNotAllowedException(DriveLetter? driveLetter)
    : base(driveLetter.HasValue
      ? "The drive cannot be created because there are no free drive letters."
      : "The drive cannot be created because specified drive letter is in use.")
  {
    this.DriveLetter = driveLetter;
  }
}

public class TooLowSizeException : Exception
{
  public ByteSize Size { get; }

  public TooLowSizeException(ByteSize size)
    : base("The drive cannot be created because too low size specifed.")
  {
    this.Size = size;
  }
}

public class TooBigSizeException : Exception
{
  public ByteSize Size { get; }

  public TooBigSizeException(ByteSize size)
    : base("The drive cannot be created because drive size cannot be greater then total ram capacity.")
  {
    this.Size = size;
  }
}