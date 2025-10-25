using System.Collections.Generic;
using GameProtocol;
using Google.Protobuf.Collections;
using LATMath;
using LATPhysics;
using UnityCore.Base;
using UnityCore.GameModule.Battle.Config;
using UnityCore.GameModule.Battle.Data;
using UnityCore.GameModule.Battle.Logic;
using UnityEngine;

namespace UnityCore.GameModule.Battle.Manager.MainGame
{
    public class BattleHeroManager
    {
        public Transform UnitRoot;
        
        List<Hero> _heroDataList = new List<Hero>();
        
        public List<Hero> HeroDataList => _heroDataList;

        public void Init(BattleHeroDto[] battleHeroes, int mapId)
        {
            int sep = battleHeroes.Length / 2;
            for (int i = 0; i < battleHeroes.Length; i++)
            {
                HeroData hd = new HeroData()
                {
                    HeroID = battleHeroes[i].HeroId,
                    PosIndex = i,
                    UserName = battleHeroes[i].UserName,
                };

                hd.UnitCfg = GetUnitCfg(battleHeroes[i].HeroId);

                var mapCfg = Game.Config.Tables.TbMap.Get(mapId);

                Hero hero;
                if (i < sep)
                {
                    hd.TeamType = TeamType.Blue;
                    hd.BornPosition = new LATVector3((LATInt)mapCfg.BlueBorn.X, (LATInt)mapCfg.BlueBorn.Y,
                        (LATInt)mapCfg.BlueBorn.Z);
                    hero = new Hero(hd);
                }
                else
                {
                    hd.TeamType = TeamType.Red;
                    hd.BornPosition = new LATVector3((LATInt)mapCfg.RedBorn.X, (LATInt)mapCfg.RedBorn.Y,
                        (LATInt)mapCfg.RedBorn.Z);
                    hero = new Hero(hd);
                }
                
                hero.UnitRoot = UnitRoot;
                hero.LogicInit();
                _heroDataList.Add(hero);
            }
        }

        private UnitCfg GetUnitCfg(int heroId)
        {
            var cfg = Game.Config.Tables.TbUnit.Get(heroId);
            UnitCfg unitCfg = new UnitCfg();
            unitCfg.ID = cfg.Id;
            unitCfg.UnitName = cfg.UnitName;
            unitCfg.ResourceName = cfg.ResName;
            unitCfg.Hp = cfg.Hp;
            unitCfg.Def = cfg.Def;
            unitCfg.MoveSpeed = cfg.MoveSpeed;
            unitCfg.ColliderConfig = new ColliderConfig()
            {
                ColliderType = (ColliderType)cfg.ColliderType,
                Radius = (LATInt)cfg.Radius,
            };

            return unitCfg;
        }
        
        public void Tick()
        {
            foreach (var hero in _heroDataList)
            {
                hero.LogicTick();
            }
        }
        
        public void UnInit()
        {
            foreach (var hero in _heroDataList)
            {
                hero.LogicUnInit();
            }
            _heroDataList.Clear();
        }

        public void InputKey(RepeatedField<OperationKey> keyList)
        {
            for (int i = 0; i < keyList.Count; i++)
            {
                OperationKey key = keyList[i];
                MainLogicUnit hero = _heroDataList[key.OperationIndex];
                hero.InputKey(key);
            }
        }

        public MainLogicUnit GetSelfHero(int posIndex)
        {
            return _heroDataList[posIndex];
        }
    }
}