using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;

namespace BestSockets.Internal
{
    internal static class SocketWrapper
    {
        public static Socket Create()
        {
            return new Socket(SocketType.Stream, ProtocolType.Tcp);
        }

        public static void ReceiveAllAsync(
            Socket socket,
            byte[] buffer,
            List<byte> data,
            Action<object> callback,
            object state)
        {
            var newState = new StateObject
            {
                Socket = socket,
                Buffer = buffer,
                Data = data,
                Callback = callback,
                OldState = state
            };

            socket.BeginReceive(buffer, 0, buffer.Length,
                SocketFlags.None, ReceiveCallback, newState);
        }

        private class StateObject
        {
            public Socket Socket;

            public byte[] Buffer;
            public List<byte> Data;

            public Action<object> Callback;
            public object OldState;
        }

        private static void ReceiveCallback(IAsyncResult ar)
        {
            var state = (StateObject)ar.AsyncState;
            var socket = state.Socket;
            var buffer = state.Buffer;

            var countOfBytesRead = socket.EndReceive(ar);

            if (countOfBytesRead > 0)
            {
                state.Data.AddRange(buffer.Take(countOfBytesRead));
                if (socket.Available > 0)
                {
                    socket.BeginReceive(buffer, 0, buffer.Length,
                        SocketFlags.None, ReceiveCallback, state);
                    return;
                }
            }

            state.Callback(state.OldState);
        }
    }
}
