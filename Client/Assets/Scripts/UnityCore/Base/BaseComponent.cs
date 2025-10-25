using System;
using System.Collections;
using LATLog;
using UnityCore.GameModule.GameFlow;
using UnityCore.ObjectPool;
using UnityCore.ResourceSystem;
using UnityEngine;

namespace UnityCore.Base
{
    public class BaseComponent: LatComponent
    {
        [SerializeField] private int frameRate = 30;
        [SerializeField] private float gameSpeed = 1f;
        [SerializeField] private bool runInBackground = true;
        [SerializeField] private bool neverSleep = true;

        public int FrameRate
        {
            get => frameRate;
            set => Application.targetFrameRate = frameRate = value;
        }
        
        public float GameSpeed
        {
            get => gameSpeed;
            set => Time.timeScale = gameSpeed = value >= 0f ? value : 0f;
        }
        
        public bool IsGamePaused => gameSpeed <= 0f;
        
        public bool IsNormalGameSpeed => Math.Abs(gameSpeed - 1f) < 0.01f;
        
        public bool RunInBackground
        {
            get => runInBackground;
            set => Application.runInBackground = runInBackground = value;
        }
        
        public bool NeverSleep
        {
            get => neverSleep;
            set
            {
                neverSleep = value;
                Screen.sleepTimeout = value ? SleepTimeout.NeverSleep : SleepTimeout.SystemSetting;
            }
        }
        
        private GameFlowManager _gameFlowManager;

        protected override void Awake()
        {
            base.Awake();
            
            InitLogger();

            Application.targetFrameRate = frameRate;
            Time.timeScale = gameSpeed;
            Application.runInBackground = runInBackground;
            Screen.sleepTimeout = neverSleep ? SleepTimeout.NeverSleep : SleepTimeout.SystemSetting;
            
#if UNITY_5_6_OR_NEWER
            Application.lowMemory += OnLowMemory;
#endif
            
            Application.wantsToQuit += () =>
            {
                GameModuleManager.Shutdown();
                return true; // Allow the application to quit
            };
            
            _gameFlowManager = GameModuleManager.GetModule<GameFlowManager>();
            _gameFlowManager.Init();
            
            IsInit = true;
        }

        private void OnEnable()
        {
            _gameFlowManager.BindEvent();
        }
        
        private void OnDisable()
        {
            _gameFlowManager.UnbindEvent();
        }

        private void Update()
        {
            GameModuleManager.Update(Time.deltaTime, Time.unscaledDeltaTime);
        }

        private void FixedUpdate()
        {
            GameModuleManager.FixedUpdate(Time.fixedDeltaTime, Time.unscaledDeltaTime);
        }

        private void InitLogger()
        {
            LogConfig logConfig = new LogConfig()
            {
                EnableLog = true,
                LogPrefix = "[Game] ",
                EnableTime = true,
                LogSeparator = " >>>>>>>> ",
                EnableThreadId = true,
                EnableSaveToFile = false,
                EnableCover = true,
                LoggerType = LoggerType.Unity,
                SavePath = Application.persistentDataPath + "/Log/",
                SaveName = "ClientLog.log"
            };
            
            GameDebug.InitSettings(logConfig);
        }
        
        private void OnLowMemory()
        {
            GameDebug.LogWarning("Low Memory!!!!!!!!");
            
            GameDebug.LogWarning("Low memory reported...");

            ObjectPoolComponent objectPoolComponent = GameEntry.GetComponent<ObjectPoolComponent>();
            if (objectPoolComponent != null)
            {
                objectPoolComponent.ReleaseAllUnused();
            }

            YooAssetComponent yooAssetComponent = GameEntry.GetComponent<YooAssetComponent>();
            if (yooAssetComponent != null)
            {
                yooAssetComponent.UnloadUnusedAssets();
            }
        }
    }
}