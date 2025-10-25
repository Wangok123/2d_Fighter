using System;
using Google.Protobuf;
using UnityCore.Base;

namespace UnityCore.Network
{
    public static class NetworkManager
    {
        public static void SendMsg<T>(ushort protocolId, T message) where T : IMessage
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message), "Message cannot be null");
            }

            Game.Network.SendMsg(protocolId, message);
        }
    }
}