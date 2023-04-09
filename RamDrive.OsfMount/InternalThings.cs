using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace RamDrive.OsfMount;

/// <summary>
/// Internal extension methods.
/// </summary>
internal static class Extensions
{
#if NETFRAMEWORK
  /// <summary>
  /// Process.WaitForExitAsync not available for net4.8, this code makes method available.
  /// </summary>
  /// <param name="process">Process.</param>
  /// <param name="cancellationToken">Cancellation token.</param>
  /// <returns>Awaitable.</returns>
  internal static async Task WaitForExitAsync(this Process process, CancellationToken cancellationToken = default)
  {
    var tcs = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);
    void ProcessExited(object? sender, EventArgs e) => _ = tcs.TrySetResult(process.ExitCode);
    try
    {
      process.EnableRaisingEvents = true;
    }
    catch (InvalidOperationException) when (process.HasExited)
    {
      // ignoring, don't care
    }

    using (cancellationToken.Register(() => tcs.TrySetCanceled()))
    {
      process.Exited += ProcessExited;
      try
      {
        if (process.HasExited)
        {
          // ! result of tcs.Task never used
          _ = tcs.TrySetResult(null!);
        }

        _ = await tcs.Task.ConfigureAwait(false);
      }
      finally
      {
        process.Exited -= ProcessExited;
      }
    }
  }
#endif

  internal delegate bool TryParse<T>(string str, out T @out);

  internal static T? Pipe<T>(this string str, TryParse<T> parseFunc)
    => parseFunc(str, out var @out) ? @out : default;
}
