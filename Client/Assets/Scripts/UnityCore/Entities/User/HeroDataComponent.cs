using System.Collections.Generic;
using LATLog;
using UnityCore.Base;
using UnityCore.Entities.Core;

namespace UnityCore.Entities.User
{
    public class HeroDataComponent : Component
    {
        public List<int> HeroSelectDataList { get; }

        public HeroDataComponent()
        {
            HeroSelectDataList = new List<int>();
        }
        
        public List<HeroData> GetHeroSelectDataList()
        {
            var heroSelectDataList = new List<HeroData>();
            foreach (var heroId in HeroSelectDataList)
            {
                heroSelectDataList.Add(new HeroData(heroId));
            }
            return heroSelectDataList;
        }
    }

    public class HeroData
    {
        public int HeroId { get; }
        public string HeroName { get; }
        public string HeroIcon { get; } 

        public HeroData(int heroId)
        {
            HeroId = heroId;
            var data = Game.Config.Tables.TbUnit.Get(heroId);
            if (data == null)
            {
                GameDebug.LogError($"HeroData: Hero config not found for ID {heroId}");
                return;
            }

            HeroName = data.UnitName;
            HeroIcon = data.ResName;
        }
    }
}