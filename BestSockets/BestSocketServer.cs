using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace BestSockets
{
    public class BestSocketServer<TReceivedData, TSentData> : IDisposable
    {
        public BestSocketServer()
        {
            _server = new Socket(SocketType.Stream, ProtocolType.Tcp);
        }

        public void Start(string ip, int port, Func<TReceivedData, TSentData> onRequest)
        {
            var endPoint = new IPEndPoint(IPAddress.Parse(ip), port);
            _server.Bind(endPoint);
            _server.Listen(MaxQueueLength);

            var state = new StateObject(_server, null, onRequest);
            _server.BeginAccept(AcceptCallback, state);
        }

        public void Close()
        {
            _server.Close();
        }

        public void Dispose()
        {
            _server.Dispose();
        }

        public static BestSocketServer<TReceivedData, TSentData>
            StartListening(string ip, int port, Func<TReceivedData, TSentData> onRequest)
        {
            var listener = new BestSocketServer<TReceivedData, TSentData>();
            listener.Start(ip, port, onRequest);
            return listener;
        }

        private readonly Socket _server;

        private class StateObject
        {
            public StateObject(
                Socket server,
                Socket handler,
                Func<TReceivedData, TSentData> onRequest)
            {
                Server = server;
                Handler = handler;
                OnRequest = onRequest;
            }

            public Socket Server;
            public Socket Handler;
            public Func<TReceivedData, TSentData> OnRequest;

            public byte[] Buffer = new byte[BufferSize];
            public List<byte> Data = new List<byte>();
            public const int BufferSize = 1024;
        }

        private const int MaxQueueLength = 100;

        private void AcceptCallback(IAsyncResult ar)
        {
            var state = (StateObject)ar.AsyncState;
            var handler = state.Server.EndAccept(ar);
            state.Handler = handler;

            handler.BeginReceive(state.Buffer, 0, state.Buffer.Length,
                SocketFlags.None, ReceiveCallback, state);
        }

        private static void ReceiveCallback(IAsyncResult ar)
        {
            var state = (StateObject)ar.AsyncState;
            var handler = state.Handler;
            var buffer = state.Buffer;

            var countOfBytesRead = handler.EndReceive(ar);

            if (countOfBytesRead > 0)
            {
                state.Data.AddRange(buffer.Take(countOfBytesRead));
                if (handler.Available > 0)
                {
                    handler.BeginReceive(buffer, 0, buffer.Length,
                        SocketFlags.None, ReceiveCallback, state);
                    return;
                }
            }

            SendResponse(state);
        }

        private static void SendResponse(StateObject state)
        {
            var request = (TReceivedData)ObjectSerializer.Deserialize(state.Data.ToArray());

            var response = state.OnRequest(request);
            var serialized = ObjectSerializer.Serialize(response);

            state.Handler.BeginSend(serialized, 0, serialized.Length,
                SocketFlags.None, SendCallback, state);
        }

        private static void SendCallback(IAsyncResult ar)
        {
            var state = (StateObject)ar.AsyncState;
            var handler = state.Handler;

            handler.EndSend(ar);
            handler.Close();
        }
    }
}