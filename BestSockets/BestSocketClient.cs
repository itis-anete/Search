using BestSockets.Internal;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;

namespace BestSockets
{
    public static class BestSocketClient<TReceivedData, TSentData>
    {
        public static TReceivedData Send(string ip, int port, TSentData data)
        {
            var waitHandle = new AutoResetEvent(false);
            var response = default(TReceivedData);

            SendAsync(ip, port, data, response_ =>
            {
                response = response_;
                waitHandle.Set();
            });
            waitHandle.WaitOne();

            return response;
        }

        public static void SendAsync(string ip, int port, TSentData data, Action<TReceivedData> onResponse)
        {
            var socket = SocketWrapper.Create();
            socket.BeginConnect(ip, port, ConnectCallback, new StateObject(socket, data, onResponse));
        }

        private class StateObject
        {
            public StateObject(Socket socket, object sentData, Action<TReceivedData> onResponse)
            {
                Socket = socket;
                SentData = sentData;
                OnResponse = onResponse;
            }

            public Socket Socket;
            public object SentData;
            public Action<TReceivedData> OnResponse;

            public byte[] Buffer = new byte[BufferSize];
            public List<byte> Data = new List<byte>();
            public const int BufferSize = 1024;
        }

        private static void ConnectCallback(IAsyncResult ar)
        {
            var state = (StateObject)ar.AsyncState;
            var socket = state.Socket;
            socket.EndConnect(ar);

            var serialized = ObjectSerializer.Serialize(state.SentData);
            socket.BeginSend(serialized, 0, serialized.Length,
                SocketFlags.None, SendCallback, state);
        }

        private static void SendCallback(IAsyncResult ar)
        {
            var state = (StateObject)ar.AsyncState;
            var socket = state.Socket;
            socket.EndSend(ar);

            SocketWrapper.ReceiveAllAsync(socket, state.Buffer, state.Data, ReceiveCallback, state);
        }

        private static void ReceiveCallback(object obj)
        {
            var state = (StateObject)obj;

            var response = (TReceivedData)ObjectSerializer.Deserialize(state.Data.ToArray());
            state.OnResponse(response);

            state.Socket.Close();
        }
    }
}
