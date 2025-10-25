using UnityCore.EventSystem;

namespace UnityCore.EventDefine
{
    public class UIEventDefine
    {
        public class OpenUISuccess : EventDefineBase
        {
            public static void SendEventMessage()
            {
                var msg = new OpenUISuccess();
                UniEvent.SendMessage(msg);
            }
        }

        public class OpenUIFailed : EventDefineBase
        {
            public static void SendEventMessage()
            {
                var msg = new OpenUIFailed();
                UniEvent.SendMessage(msg);
            }
        }
        
        public class CloseUISuccess : EventDefineBase
        {
            public static void SendEventMessage()
            {
                var msg = new CloseUISuccess();
                UniEvent.SendMessage(msg);
            }
        }
    }
}