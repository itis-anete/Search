using BestSockets.Internal;
using System;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace BestSockets
{
    public class BestSocketClient<TReceivedData, TSentData> : BestSocketBase<TReceivedData, TSentData>
    {
        public BestSocketClient(string ip, int port, IObjectSerializer objectSerializer = null)
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
    }
}
