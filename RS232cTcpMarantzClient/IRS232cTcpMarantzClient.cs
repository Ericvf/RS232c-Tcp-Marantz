using System.Threading.Tasks;

namespace RS232cTcpMarantz
{
    public interface IRS232cTcpMarantzClient
    {
        Task Start(string ipAddress, int port);

        void Stop();

        bool IsConnected();

        string Get(string command);
    }
}