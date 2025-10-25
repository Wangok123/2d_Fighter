using UnityCore.GameModule.Battle.Config;
using UnityCore.GameModule.Battle.Logic;
using UnityEngine;

namespace UnityCore.GameModule.Battle.Data
{
    public class Hero : MainLogicUnit
    {
        public int HeroID;
        public int PosIndex; // 英雄位置索引
        public string UserName; // 用户名
        
        public Hero(HeroData data) : base(data)
        {
            HeroID = data.HeroID;
            PosIndex = data.PosIndex;
            UserName = data.UserName;

            // 初始化属性
            UnitType = UnitType.Hero;
            UnitName = data.UnitCfg.UnitName + " - " + UserName;
        }
    }
}