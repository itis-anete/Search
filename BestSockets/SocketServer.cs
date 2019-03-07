using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace BestSockets
{
    public class SocketServer<TReceivedData, TSentData> : SocketBase<TReceivedData, TSentData>, IDisposable
    {
        public SocketServer(string ip, int port, IObjectSerializer objectSerializer = null)
            : base(ip, port, objectSerializer) { }

        public void Start(Func<TReceivedData, TSentData> onRequest)
        {
            InitializeSocket();

            var endPoint = new IPEndPoint(_ip, _port);
            _socket.Bind(endPoint);
            _socket.Listen(MaxQueueLength);

            _isListening = true;
            _listeningTask = ListenAsync();
        }

        public void Stop()
        {
            _isListening = false;
            _listeningTask.Wait();

            FinalizeSocket();
        }

        public void Dispose()
        {
            Stop();
        }

        public static SocketServer<TReceivedData, TSentData> StartNew(
            string ip,
            int port,
            Func<TReceivedData, TSentData> onRequest,
            IObjectSerializer objectSerializer = null)
        {
            var server = new SocketServer<TReceivedData, TSentData>(ip, port, objectSerializer);
            server.Start(onRequest);
            return server;
        }

        private bool _isListening;
        private Task _listeningTask;
        private readonly Func<TReceivedData, TSentData> _onRequest;

        private const int MaxQueueLength = 100;

        private async Task ListenAsync()
        {
            while (_isListening)
            {
                var handler = await _socket.AcceptAsync();
                var request = await ReceiveDataAsync(handler);
                var response = _onRequest(request);
                await SendDataAsync(response, handler);
                handler.Close();
            }
        }
    }
}