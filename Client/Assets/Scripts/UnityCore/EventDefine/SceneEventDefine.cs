using UnityCore.EventSystem;

namespace UnityCore.EventDefine
{
    public class SceneEventDefine
    {
        public class ChangeToHomeScene : EventDefineBase
        {
            public static void SendEventMessage()
            {
                var msg = new ChangeToHomeScene();
                UniEvent.SendMessage(msg);
            }
        }

        public class ChangeToBattleScene : EventDefineBase
        {
            public static void SendEventMessage()
            {
                var msg = new ChangeToBattleScene();
                UniEvent.SendMessage(msg);
            }
        }
    }
}