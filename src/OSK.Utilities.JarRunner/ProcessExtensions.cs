using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace OSK.Utilities.JarRunner
{
    public static class ProcessExtensions
    {
        public static Task RunAsync(this Process process)
        {
            var tcs = new TaskCompletionSource<int>();

            process.EnableRaisingEvents = true;
            process.Exited += (sender, args) =>
            {
                tcs.SetResult(process.ExitCode);
            };

            process.Start();

            return tcs.Task;
        }

        public static async Task<byte[]> GetErrorBytesAsync(this Process process)
        {
            await using var memoryStream = new MemoryStream();
            await process.StandardError.BaseStream.CopyToAsync(memoryStream);

            return memoryStream.ToArray();
        }
    }
}
