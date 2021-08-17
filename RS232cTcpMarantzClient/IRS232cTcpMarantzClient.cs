using System.Threading.Tasks;

namespace RS232cTcpMarantz
{
    public interface IRS232cTcpMarantzClient
    {
        Task<string> Get(string command);

        bool IsConnected();

        Task Start(string ipAddress, int port);

        Task Stop();

        Task<string> SendCommandAndGetResponse(string command);
    }
}