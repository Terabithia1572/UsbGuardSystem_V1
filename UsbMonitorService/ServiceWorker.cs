using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.IO.Pipes;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace UsbMonitorService
{
    public class ServiceWorker : BackgroundService
    {
        private readonly ILogger<ServiceWorker> _logger;
        private readonly UsbLogRepository _repo;
        private const string PipeName = "UsbMonitorPipe";

        public ServiceWorker(ILogger<ServiceWorker> logger, UsbLogRepository repo)
        {
            _logger = logger;
            _repo = repo;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Directory.CreateDirectory(@"C:\ProgramData\UsbMonitor");
            _logger.LogInformation("UsbMonitorService started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                using var server = new NamedPipeServerStream(PipeName, PipeDirection.InOut, 5, PipeTransmissionMode.Message, PipeOptions.Asynchronous);
                await server.WaitForConnectionAsync(stoppingToken);

                using var reader = new StreamReader(server);
                string? json = await reader.ReadToEndAsync();
                if (!string.IsNullOrWhiteSpace(json))
                {
                    try
                    {
                        var log = JsonSerializer.Deserialize<UsbLogEntry>(json);
                        if (log != null)
                        {
                            _repo.InsertLog(log);
                            _logger.LogInformation($"Log added for {log.Username} - {log.FileName}");
                        }
                    }
                    catch { /* malformed json */ }
                }
                server.Disconnect();
            }
        }
    }
}
