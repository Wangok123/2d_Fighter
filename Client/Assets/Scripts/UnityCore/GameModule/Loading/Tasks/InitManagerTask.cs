using System.Collections;
using GameProtocol;
using UnityCore.Base;
using UnityCore.GameModule.Battle.Manager;
using UnityCore.GameModule.Battle.Manager.MainGame;

namespace UnityCore.GameModule.Loading.Tasks
{
    /// <summary>
    /// 初始化游戏状态下各个管理器
    /// </summary>
    public class InitManagerTask : ILoadingTask
    {
        public class InitData
        {
            public BattleHeroDto[] HeroDataArray;
            public int MapID;
            public int PosIndex;
        }

        private bool _isDone;
        public float Progress { get; }
        public int Weight => 1;
        public bool IsDone => _isDone;

        private InitData _data;
        
        public InitManagerTask(InitData data)
        {
            _data = data;
        }
        
        public IEnumerator Execute()
        {
            var gameManager = GameModuleManager.GetModule<MainGameManager>();
            gameManager.InitPhysicEnv();
            gameManager.InitBattleHeros(_data.HeroDataArray, _data.MapID);
            gameManager.InitCamera(_data.PosIndex);
            gameManager.InitPlayerIndex(_data.PosIndex);
            yield return null;
            _isDone = true;
        }
    }
}