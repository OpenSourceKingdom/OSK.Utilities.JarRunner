using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OSK.Utilities.JarRunner
{
    public abstract class JarCommand<TResult>
    {
        public async Task<TResult> ExecuteAsync(string jarFilePath, CancellationToken cancellationToken = default)
        {
            try
            {
                await PrepareAsync(cancellationToken);
                using var process = new Process()
                {
                    EnableRaisingEvents = true,
                    StartInfo = new ProcessStartInfo("java")
                    {
                        RedirectStandardInput = true,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        WindowStyle = ProcessWindowStyle.Hidden,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        Arguments = $"-jar \"{jarFilePath}\" {string.Join(" ", GetArgumentList())}"
                    },
                };

                process.ErrorDataReceived += (sender, args) => throw new InvalidOperationException(
                    $"An error executing the jar command of type {GetType().FullName}: {args.Data}");

                await process.RunAsync();
                var error = await process.GetErrorBytesAsync();
                if (error.Any())
                {
                    var message = Encoding.UTF8.GetString(error);
                    throw new InvalidOperationException($"Error occurred using the jar command of type {GetType().FullName}. Error {message}");
                }

                return await GetResultAsync(process, cancellationToken);
            }
            finally
            {
                CleanUp();
            }
        }

        protected abstract ValueTask<TResult> GetResultAsync(Process process, CancellationToken cancellationToken);

        protected virtual ValueTask PrepareAsync(CancellationToken cancellationToken = default)
        {
            return new ValueTask();
        }

        protected virtual IEnumerable<string> GetArgumentList()
        {
            return Enumerable.Empty<string>();
        }

        protected virtual void CleanUp()
        {
        }
    }
}
