using System.ComponentModel;
using System.Text;
using Google.Protobuf;

namespace LatProtocol;

public static class ProtobufHelper
{
    public static byte[] ToBytes(object message)
    {
        return ((IMessage)message).ToByteArray();
    }

    public static void ToStream(object message, MemoryStream stream)
    {
        ((IMessage)message).WriteTo(stream);
    }
    
    public static object FromBytes(Type type, byte[] bytes)
    {
        object message = Activator.CreateInstance(type);
        ((IMessage)message).MergeFrom(bytes);
        ISupportInitialize? init = message as ISupportInitialize;
        if (init == null)
        {
            return message;
        }

        init.EndInit();
        return message;
    }

    public static object FromBytes(Type type, byte[] bytes, int index, int count)
    {
        object message = Activator.CreateInstance(type);
        ((IMessage)message).MergeFrom(bytes, index, count);
        ISupportInitialize? init = message as ISupportInitialize;
        if (init == null)
        {
            return message;
        }

        init.EndInit();
        return message;
    }

    public static object FromBytes(object instance, byte[] bytes, int index, int count)
    {
        object message = instance;
        ((IMessage)message).MergeFrom(bytes, index, count);
        ISupportInitialize? init = instance as ISupportInitialize;
        if (init == null)
        {
            return instance;
        }

        init.EndInit();
        return instance;
    }

    public static object FromStream(Type type, MemoryStream stream)
    {
        object message = Activator.CreateInstance(type);
        ((IMessage)message).MergeFrom(stream.GetBuffer(), (int)stream.Position, (int)stream.Length);
        ISupportInitialize? init = message as ISupportInitialize;
        if (init == null)
        {
            return message;
        }

        init.EndInit();
        return message;
    }

    public static object FromStream(object instance, MemoryStream stream)
    {
        object message = instance;
        ((IMessage)message).MergeFrom(stream.GetBuffer(), (int)stream.Position, (int)stream.Length);
        ISupportInitialize? init = instance as ISupportInitialize;
        if (init == null)
        {
            return instance;
        }

        init.EndInit();
        return instance;
    }
    
    public static void PrintBytes(string tag, byte[] bytes, int startIndex=0, int endIndex=-1)
    {
        StringBuilder sb = new StringBuilder();
        sb.Append("[");
        sb.Append(tag);
        sb.Append("] ");
        sb.AppendFormat("len:{0} >>> ", bytes.Length);
        if (endIndex == -1)
        {
            endIndex = bytes.Length;
        }

        for (var i = 0; i < bytes.Length; i++)
        {
            if (i < startIndex)
                continue;
            if (i > endIndex)
                break;
            sb.Append(bytes[i]);
            sb.Append(",");
        }

        Console.WriteLine(sb.ToString());
    }
}