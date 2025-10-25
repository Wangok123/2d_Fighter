using Core;
using GameProtocol;

namespace UnityCore.GameModule.Playing
{
    public class PlayingManager : CoreModule
    {
        private BattleHeroDto[] _heroDtoList;

        public void Init(BattleHeroDto[] heroDtoList)
        {
            _heroDtoList = heroDtoList;
            // TODO: 地图初始化配置
        }

        internal override void Update(float elapseSeconds, float realElapseSeconds)
        {
        }

        internal override void Shutdown()
        {
        }
    }
}