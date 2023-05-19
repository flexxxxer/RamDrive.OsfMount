﻿using System;
using System.Drawing;
using System.Runtime.Serialization;

using ByteSizeLib;

namespace RamDrive.OsfMount.ObjectOriented;
#pragma warning disable RCS1194 // Implement exception constructors.

/// <summary>
/// Error that can be generated by <seealso cref="RamDrive.New(ByteSize, FileSystemType, DriveLetter?)"/>
///  or <seealso cref="OsfMountRamDrive.New(ByteSize, DriveLetter?, FileSystemType)"/>.
/// </summary>
[Serializable]
public class DriveLetterInUseOrNotAllowedException : Exception
{
  private readonly DriveLetter? driveLetter;

  /// <summary>
  /// Initializes a new instance of the <see cref="DriveLetterInUseOrNotAllowedException"/> class.
  /// </summary>
  public DriveLetterInUseOrNotAllowedException()
  {
  }

  /// <summary>
  /// Initializes a new instance of the <see cref="DriveLetterInUseOrNotAllowedException"/> class.
  /// </summary>
  /// <param name="driveLetter">Drve letter.</param>
  public DriveLetterInUseOrNotAllowedException(DriveLetter? driveLetter)
  {
    this.driveLetter = driveLetter;
  }

  /// <inheritdoc/>
  public override string Message => this.driveLetter.HasValue
      ? "The drive cannot be created because there are no free drive letters."
      : "The drive cannot be created because specified drive letter is in use.";

  /// <summary>
  /// Initializes a new instance of the <see cref="DriveLetterInUseOrNotAllowedException"/> class.
  /// </summary>
  /// <param name="serializationInfo">Serialization info.</param>
  /// <param name="streamingContext">Streaming context.</param>
  protected DriveLetterInUseOrNotAllowedException(SerializationInfo serializationInfo, StreamingContext streamingContext)
    : base(serializationInfo, streamingContext)
  {
    this.driveLetter = (DriveLetter?)serializationInfo.GetValue(nameof(this.driveLetter), typeof(DriveLetter?));
  }

  /// <inheritdoc/>
  public override void GetObjectData(SerializationInfo info, StreamingContext context)
  {
    info.AddValue(nameof(this.driveLetter), this.driveLetter, typeof(DriveLetter?));
    base.GetObjectData(info, context);
  }
}

/// <summary>
/// Error that can be generated by <seealso cref="RamDrive.New(ByteSize, FileSystemType, DriveLetter?)"/>
///  or <seealso cref="OsfMountRamDrive.New(ByteSize, DriveLetter?, FileSystemType)"/>.
/// </summary>
[Serializable]
public class TooLowSizeException : Exception
{
  private readonly ByteSize size;

  /// <summary>
  /// Initializes a new instance of the <see cref="TooLowSizeException"/> class.
  /// </summary>
  public TooLowSizeException()
  {
  }

  /// <summary>
  /// Initializes a new instance of the <see cref="TooLowSizeException"/> class.
  /// </summary>
  /// <param name="size">Drve size.</param>
  public TooLowSizeException(ByteSize size)
  {
    this.size = size;
  }

  /// <inheritdoc/>
  public override string Message => "The drive cannot be created because too low size specifed.";

  /// <summary>
  /// Initializes a new instance of the <see cref="TooLowSizeException"/> class.
  /// </summary>
  /// <param name="serializationInfo">Serialization info.</param>
  /// <param name="streamingContext">Streaming context.</param>
  protected TooLowSizeException(SerializationInfo serializationInfo, StreamingContext streamingContext)
    : base(serializationInfo, streamingContext)
  {
    // ! dont care when incomplete data
    this.size = new ByteSize((long)serializationInfo.GetValue(nameof(this.size.Bits), typeof(long))!);
  }

  /// <inheritdoc/>
  public override void GetObjectData(SerializationInfo info, StreamingContext context)
  {
    info.AddValue(nameof(this.size.Bits), this.size, typeof(long));
    base.GetObjectData(info, context);
  }
}

/// <summary>
/// Error that can be generated by <seealso cref="RamDrive.New(ByteSize, FileSystemType, DriveLetter?)"/>
///  or <seealso cref="OsfMountRamDrive.New(ByteSize, DriveLetter?, FileSystemType)"/>.
/// </summary>
[Serializable]
public class TooBigSizeException : Exception
{
  private readonly ByteSize size;

  /// <summary>
  /// Initializes a new instance of the <see cref="TooBigSizeException"/> class.
  /// </summary>
  public TooBigSizeException()
  {
  }

  /// <summary>
  /// Initializes a new instance of the <see cref="TooBigSizeException"/> class.
  /// </summary>
  /// <param name="size">Drve size.</param>
  public TooBigSizeException(ByteSize size)
  {
    this.size = size;
  }

  /// <inheritdoc/>
  public override string Message => "The drive cannot be created because drive size cannot be greater then total ram capacity.";

  /// <summary>
  /// Initializes a new instance of the <see cref="TooBigSizeException"/> class.
  /// </summary>
  /// <param name="serializationInfo">Serialization info.</param>
  /// <param name="streamingContext">Streaming context.</param>
  protected TooBigSizeException(SerializationInfo serializationInfo, StreamingContext streamingContext)
    : base(serializationInfo, streamingContext)
  {
    // ! dont care when incomplete data
    this.size = new ByteSize((long)serializationInfo.GetValue(nameof(this.size.Bits), typeof(long))!);
  }

  /// <inheritdoc/>
  public override void GetObjectData(SerializationInfo info, StreamingContext context)
  {
    info.AddValue(nameof(this.size.Bits), this.size, typeof(long));
    base.GetObjectData(info, context);
  }
}