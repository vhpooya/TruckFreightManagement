using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;


namespace TruckFreight.Infrastructure.Services
{
    public class ClamAvService : IVirusScanService
    {
        private readonly string _host;
        private readonly int _port;
        private readonly int _timeout;
        private readonly ILogger<ClamAvService> _logger;

        public ClamAvService(IConfiguration configuration, ILogger<ClamAvService> logger)
        {
            _logger = logger;
            _host = configuration["VirusScan:ClamAv:Host"] ?? "localhost";
            _port = configuration.GetValue<int>("VirusScan:ClamAv:Port", 3310);
            _timeout = configuration.GetValue<int>("VirusScan:ClamAv:Timeout", 30000);
        }

        public async Task<bool> ScanFileAsync(Stream fileStream, string fileName)
        {
            try
            {
                if (!await IsServiceAvailableAsync())
                {
                    _logger.LogWarning("ClamAV service is not available");
                    return false;
                }

                using var client = new TcpClient();
                await client.ConnectAsync(_host, _port);
                client.ReceiveTimeout = _timeout;
                client.SendTimeout = _timeout;

                using var stream = client.GetStream();
                
                // Send INSTREAM command
                var command = Encoding.ASCII.GetBytes("zINSTREAM\0");
                await stream.WriteAsync(command, 0, command.Length);

                // Send file size
                var size = BitConverter.GetBytes((uint)fileStream.Length);
                await stream.WriteAsync(size, 0, size.Length);

                // Send file content
                var buffer = new byte[8192];
                int bytesRead;
                while ((bytesRead = await fileStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    var chunkSize = BitConverter.GetBytes((uint)bytesRead);
                    await stream.WriteAsync(chunkSize, 0, chunkSize.Length);
                    await stream.WriteAsync(buffer, 0, bytesRead);
                }

                // Send end of file marker
                var endMarker = new byte[] { 0, 0, 0, 0 };
                await stream.WriteAsync(endMarker, 0, endMarker.Length);

                // Read response
                var responseBuffer = new byte[1024];
                var responseLength = await stream.ReadAsync(responseBuffer, 0, responseBuffer.Length);
                var response = Encoding.ASCII.GetString(responseBuffer, 0, responseLength);

                return response.Contains("OK");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error scanning file {FileName} for viruses", fileName);
                return false;
            }
        }

        public async Task<bool> IsServiceAvailableAsync()
        {
            try
            {
                using var client = new TcpClient();
                await client.ConnectAsync(_host, _port);
                using var stream = client.GetStream();
                
                var command = Encoding.ASCII.GetBytes("zPING\0");
                await stream.WriteAsync(command, 0, command.Length);

                var buffer = new byte[1024];
                var bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                var response = Encoding.ASCII.GetString(buffer, 0, bytesRead);

                return response.Contains("PONG");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking ClamAV service availability");
                return false;
            }
        }
    }
} 