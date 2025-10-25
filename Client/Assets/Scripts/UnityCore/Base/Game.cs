using Configs;
using UnityCore.Audio;
using UnityCore.Entities;
using UnityCore.EventSystem;
using UnityCore.Input;
using UnityCore.Network;
using UnityCore.ResourceSystem;
using UnityCore.UI.Core;

namespace UnityCore.Base
{
    public static class Game
    {
        public static BaseComponent Base => GameEntry.GetComponent<BaseComponent>();
        public static UIComponent UI => GameEntry.GetComponent<UIComponent>();
        public static UniEventComponent Event => GameEntry.GetComponent<UniEventComponent>();
        public static NetworkComponent Network => GameEntry.GetComponent<NetworkComponent>();
        public static YooAssetComponent YooAsset => GameEntry.GetComponent<YooAssetComponent>();
        public static AudioComponent Audio => GameEntry.GetComponent<AudioComponent>();
        public static EntityComponent World => GameEntry.GetComponent<EntityComponent>();
        public static ConfigComponent Config => GameEntry.GetComponent<ConfigComponent>();
        public static InputComponent Input => GameEntry.GetComponent<InputComponent>();

        // 获取当前平台
        public static string Platform
        {
            get
            {
#if UNITY_EDITOR
                return "Editor";
#elif UNITY_ANDROID
                return "Android";
#elif UNITY_IOS
                return "IOS";
#elif UNITY_STANDALONE_WIN
                return "Windows";
#elif UNITY_STANDALONE_OSX
                return "MacOS";
#else
                return "Unknown";
#endif
            }
        }
    }
}