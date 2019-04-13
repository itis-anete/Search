using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace BestSockets
{
    public class SocketServer<TSentData> : SocketBase<string, TSentData>, IDisposable
    {
        public SocketServer(string ip, int port) : base(ip, port) { }

        public void Start(Func<string, TSentData> onRequest)
        {
            if (_isListening)
                return;

            InitializeSocket();
            _onRequest = onRequest;

            var endPoint = new IPEndPoint(_ip, _port);
            _socket.Bind(endPoint);
            _socket.Listen(MaxQueueLength);

            _isListening = true;
            ListenAsync();
        }

        public void Stop()
        {
            _isListening = false;
            FinalizeSocket();
        }

        public void Dispose()
        {
            Stop();
        }

        public static SocketServer<TSentData> StartNew(string ip, int port, Func<string, TSentData> onRequest)
        {
            var server = new SocketServer<TSentData>(ip, port);
            server.Start(onRequest);
            return server;
        }

        private bool _isListening;
        private Func<string, TSentData> _onRequest;

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