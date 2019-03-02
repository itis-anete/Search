using BestSockets.Internal;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace BestSockets
{
    public class BestSocketServer<TReceivedData, TSentData> : IDisposable
    {
        public BestSocketServer()
        {
            _server = SocketWrapper.Create();
        }

        public void Start(string ip, int port, Func<TReceivedData, TSentData> onRequest)
        {
            var endPoint = new IPEndPoint(IPAddress.Parse(ip), port);
            _server.Bind(endPoint);
            _server.Listen(MaxQueueLength);

            var state = new StateObject(_server, onRequest);
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

        private class StateObject
        {
            public StateObject(Socket server, Func<TReceivedData, TSentData> onRequest)
            {
                Server = server;
                OnRequest = onRequest;
            }

            public Socket Server;
            public Socket Handler;
            public Func<TReceivedData, TSentData> OnRequest;

            public byte[] Buffer = new byte[BufferSize];
            public List<byte> Data = new List<byte>();
            public const int BufferSize = 1024;
        }

        private readonly Socket _server;

        private const int MaxQueueLength = 100;

        private static void AcceptCallback(IAsyncResult ar)
        {
            var state = (StateObject)ar.AsyncState;
            var handler = state.Server.EndAccept(ar);
            state.Handler = handler;

            SocketWrapper.ReceiveAllAsync(handler, state.Buffer, state.Data, SendResponse, state);
        }

        private static void SendResponse(object obj)
        {
            var state = (StateObject)obj;

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