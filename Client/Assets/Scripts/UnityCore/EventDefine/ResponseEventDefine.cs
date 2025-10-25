using Core.ReferencePool;
using GameProtocol;
using UnityCore.EventSystem;

namespace UnityCore.EventDefine
{
    public class ResponseEventDefine
    {
        public class SelectNotificationArgs : EventDefineBase
        {
            public static void SendEventMessage()
            {
                var msg = new SelectNotificationArgs();
                UniEvent.SendMessage(msg);
            }
        }

        public class LoginSuccessArgs : EventDefineBase
        {
            public static void SendEventMessage()
            {
                var msg = new LoginSuccessArgs();
                UniEvent.SendMessage(msg);
            }
        }
        
        public class MatchStartArgs : EventDefineBase
        {
            public int PredictionTime { get; set; }
            
            public static void SendEventMessage(int time)
            {
                var msg = new MatchStartArgs();
                msg.PredictionTime = time;
                UniEvent.SendMessage(msg);
            }
        }
        
        public class ConfirmNotificationArgs : EventDefineBase
        {
            public uint RoomId;
            public bool IsDismiss;
            public ConfirmDto[] ConfirmDataArr;
            
            public static void SendEventMessage(uint roomId, bool isDismiss, ConfirmDto[] confirmDataArr)
            {
                var msg = new ConfirmNotificationArgs();
                msg.RoomId = roomId;
                msg.IsDismiss = isDismiss;
                msg.ConfirmDataArr = confirmDataArr;
                
                UniEvent.SendMessage(msg);
            }
        }
        
        public class LoadResNotificationArgs : EventDefineBase
        {
            public uint MapId;
            public int PosIndex;
            public BattleHeroDto[] HeroDataArray;
            
            public static void SendEventMessage(uint mapId, int posIndex, BattleHeroDto[] heroDataList)
            {
                var msg = new LoadResNotificationArgs
                {
                    MapId = mapId,
                    PosIndex = posIndex,
                    HeroDataArray = heroDataList
                };
                
                UniEvent.SendMessage(msg);
            }
        }

#if UNITY_EDITOR
        public class GMLoadResNotificationArgs : EventDefineBase
        {
            public uint MapId;
            public int PosIndex;
            public BattleHeroDto[] HeroDataArray;

            public static void SendEventMessage(uint mapId, int posIndex, BattleHeroDto[] heroDataList)
            {
                var msg = new GMLoadResNotificationArgs
                {
                    MapId = mapId,
                    PosIndex = posIndex,
                    HeroDataArray = heroDataList
                };

                UniEvent.SendMessage(msg);
            }

            public LoadResNotificationArgs CopyToLoadResNotificationArgs()
            {
                var args = new LoadResNotificationArgs
                {
                    MapId = MapId,
                    PosIndex = PosIndex,
                    HeroDataArray = HeroDataArray
                };
                return args;
            }
        }
#endif
        public class LoadProgressNotificationArgs : EventDefineBase
        {
            public int[] PercentList;

            public static void SendEventMessage(int[] percentList)
            {
                var msg = new LoadProgressNotificationArgs
                {
                    PercentList = percentList
                };
                
                UniEvent.SendMessage(msg);
            }
        }
        
        public class LoadFinishNotificationArgs : EventDefineBase
        {
            public static void SendEventMessage()
            {
                var msg = new LoadFinishNotificationArgs();
                UniEvent.SendMessage(msg);
            }
        }

        public class InitializePlayingArgs : EventDefineBase
        {
            public BattleHeroDto[] HeroDataArray;

            public static void SendEventMessage(BattleHeroDto[] heroDataList)
            {
                var msg = new InitializePlayingArgs
                {
                    HeroDataArray = heroDataList
                };

                UniEvent.SendMessage(msg);
            }
        }

        public class BattleStartArgs : EventDefineBase
        {
        }
    }
}