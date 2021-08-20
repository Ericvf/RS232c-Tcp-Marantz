using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RS232cTcpMarantz
{
    public sealed class RS232cTcpMarantzClient : IRS232cTcpMarantzClient, IDisposable
    {
        private readonly ILogger<RS232cTcpMarantzClient> logger;
        private const int timeout = 500;

        private TcpClient? tcpClient;
        private NetworkStream? networkStream;
        private StreamWriter? writer;

        public RS232cTcpMarantzClient(ILogger<RS232cTcpMarantzClient> logger)
        {
            this.logger = logger;
        }

        public async Task Start(string ipAddress, int port)
        {
            tcpClient = new TcpClient();

            await tcpClient.ConnectAsync(ipAddress, port);

            networkStream = tcpClient.GetStream();
            writer = new StreamWriter(networkStream, Encoding.ASCII);
            writer.AutoFlush = true;
        }

        public void Stop()
        {
            SendCommandAndGetResponse("BYE");
            networkStream?.Dispose();
            tcpClient?.Dispose();
        }

        public bool IsConnected() => tcpClient?.Connected == true;

        public string Get(string command) => SendCommandAndGetResponse(command);

        private string SendCommandAndGetResponse(string command)
        {
            logger.LogInformation("SendCommandAndGetResponse", command);

            var bytes = Encoding.ASCII.GetBytes(command + '\r');
            var chars = Encoding.ASCII.GetChars(bytes);
            writer!.Write(chars);

            return GetResponse();
        }

        private string GetResponse()
        {
            if (networkStream == null)
                return string.Empty;

            var stringBuilder = new StringBuilder();

            using var cancellationToken = new CancellationTokenSource();
            cancellationToken.CancelAfter(timeout);

            while (!networkStream.DataAvailable && !cancellationToken.IsCancellationRequested);

            while (!cancellationToken.IsCancellationRequested)
            {
                if (networkStream.DataAvailable)
                {
                    stringBuilder.Append((char)networkStream.ReadByte());
                }
            }

            return stringBuilder.ToString().Replace('\r', ' ');
        }

        public void Dispose()
        {
            networkStream?.Dispose();
            tcpClient?.Dispose();
        }
    }
}