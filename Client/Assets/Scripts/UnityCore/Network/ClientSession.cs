using System;
using LATNet;
using LatProtocol;
using UnityCore.Base;

namespace UnityCore.Network
{
    public class ClientSession : KCPSession
    {
        protected override void OnConnected()
        {
        }

        protected override void OnDisConnected()
        {
        }

        protected override void OnReceiveMsg(byte[] bytes)
        {
            ProtocolProcessor.ProcessPacket(bytes, out ushort cmdId, out byte[] message);
            Game.Network.AddMsgQueue(cmdId, message);
        }

        protected override void OnUpdate(DateTime now)
        { 
        }
    }
}