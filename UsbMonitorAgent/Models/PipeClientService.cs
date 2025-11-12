using System.IO.Pipes;
using System.Text.Json;
using System.Threading.Tasks;
using System.IO;

namespace UsbMonitorAgent
{
    public static class PipeClientService
    {
        private const string PipeName = "UsbMonitorPipe";

        public static async Task SendLogAsync(UsbLogModel log)
        {
            try
            {
                using var client = new NamedPipeClientStream(".", PipeName, PipeDirection.Out);
                await client.ConnectAsync(1500);
                using var sw = new StreamWriter(client);
                string json = JsonSerializer.Serialize(new
                {
                    Username = log.Username,
                    FileName = log.FileName,
                    SourcePath = log.SourcePath,
                    DestPath = log.DestPath,
                    DriveLabel = log.DriveLabel,
                    DriveSerial = "", // optional
                    FileSize = log.FileSize,
                    TimestampUtc = log.TimestampUtc,
                    FileHash = ""
                });
                await sw.WriteAsync(json);
            }
            catch
            {
                // service not reachable -> ignore or fallback
            }
        }

        public static async Task<bool> CheckServiceAsync()
        {
            try
            {
                using var client = new NamedPipeClientStream(".", PipeName, PipeDirection.Out);
                await client.ConnectAsync(500);
                return true;
            }
            catch { return false; }
        }
    }
}
