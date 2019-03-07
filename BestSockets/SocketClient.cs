using System;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace BestSockets
{
    public class SocketClient<TReceivedData, TSentData> : SocketBase<TReceivedData, TSentData>
    {
        public SocketClient(string ip, int port, IObjectSerializer objectSerializer = null)
            : base(ip, port, objectSerializer) { }

        public TReceivedData Send(TSentData data)
        {
            return SendAsync(data).Result;
        }

        public async Task<TReceivedData> SendAsync(TSentData data)
        {
            InitializeSocket();

            await _socket.ConnectAsync(_ip, _port);
            await SendDataAsync(data, _socket);
            var response = await ReceiveDataAsync(_socket);

            FinalizeSocket();
            return response;
        }

        public static TReceivedData Send(
            string ip,
            int port,
            TSentData data,
            IObjectSerializer objectSerializer = null)
        {
            var client = new SocketClient<TReceivedData, TSentData>(ip, port, objectSerializer);
            return client.Send(data);
        }

        public static async Task<TReceivedData> SendAsync(
            string ip,
            int port,
            TSentData data,
            IObjectSerializer objectSerializer = null)
        {
            var client = new SocketClient<TReceivedData, TSentData>(ip, port, objectSerializer);
            return await client.SendAsync(data);
        }
    }
}
