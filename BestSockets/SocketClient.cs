using System;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace BestSockets
{
    public class SocketClient<TSentData> : SocketBase<string, TSentData>
    {
        public SocketClient(string ip, int port) : base(ip, port) { }

        public string Send(TSentData data)
        {
            return SendAsync(data).Result;
        }

        public async Task<string> SendAsync(TSentData data)
        {
            InitializeSocket();

            await _socket.ConnectAsync(_ip, _port);
            await SendDataAsync(data, _socket);
            var response = await ReceiveDataAsync(_socket);

            FinalizeSocket();
            return response;
        }

        public static string Send(string ip, int port, TSentData data)
        {
            var client = new SocketClient<TSentData>(ip, port);
            return client.Send(data);
        }

        public static async Task<string> SendAsync(string ip, int port, TSentData data)
        {
            var client = new SocketClient<TSentData>(ip, port);
            return await client.SendAsync(data);
        }
    }
}
