using System;
using System.ComponentModel;
using System.IO;
using Google.Protobuf;
using LATLog;
using LatProtocol;

namespace UnityCore.Network.Dispatcher
{
    public abstract class MessageHandler<T> : IMessageHandler where T : class, new()
    {
        public uint CommandId { get; }

        protected MessageHandler(uint cmdId)
        {
            CommandId = cmdId;
        }

        public void Handle(MsgPack message)
        {
            Type type = ProtocolMapping.ProtocolMap[message.CMDID];
            if (type == null)
            {
                throw new Exception($"Protocol ID {message.CMDID} not found in mapping.");
            }

            var payload = ProtobufHelper.FromBytes(type, message.Bytes) as T;
            if (payload == null)
            {
                throw new Exception($"Failed to deserialize message of type {type.Name}.");
            }

            Process(payload);
        }

        protected abstract void Process(T message);
    }
}