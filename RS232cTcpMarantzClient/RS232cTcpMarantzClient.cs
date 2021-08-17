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
        private NetworkStream? netstream;
        private StreamWriter? writer;

        public RS232cTcpMarantzClient(ILogger<RS232cTcpMarantzClient> logger)
        {
            this.logger = logger;
        }

        public async Task Start(string ipAddress, int port)
        {
            tcpClient = new TcpClient();

            await tcpClient.ConnectAsync(ipAddress, port);

            netstream = tcpClient.GetStream();
            writer = new StreamWriter(netstream, Encoding.ASCII);
            writer.AutoFlush = true;
        }

        public async Task Stop()
        {
            await SendCommandAndGetResponse("BYE");
            netstream?.Dispose();
        }

        public Task<string> Get(string command) => SendCommandAndGetResponse(command);

        public async Task<string> SendCommandAndGetResponse(string command)
        {
            logger.LogInformation("SendCommandAndGetResponse", command);

            var bytes = Encoding.ASCII.GetBytes(command + '\r');
            var chars = Encoding.ASCII.GetChars(bytes);
            writer!.Write(chars);

            using var cancellationToken = new CancellationTokenSource();
            cancellationToken.CancelAfter(timeout);

            return await ReadOutput(netstream!, cancellationToken.Token);
        }

        private Task<string> ReadOutput(NetworkStream networkStream, CancellationToken cancellationToken)
        {
            var stringBuilder = new StringBuilder();

            while (!networkStream.DataAvailable && !cancellationToken.IsCancellationRequested);

            while (!cancellationToken.IsCancellationRequested)
            {
                if (networkStream.DataAvailable)
                {
                    stringBuilder.Append((char)networkStream.ReadByte());
                }
            }

            return Task.FromResult(stringBuilder.ToString().Replace('\r', ' '));
        }

        public bool IsConnected() => tcpClient?.Connected == true;

        public void Dispose()
        {
            netstream?.Dispose();
        }
    }
}