using System.Collections.Generic;
using LATLog;
using UnityCore.Base;
using UnityCore.GameModule.Battle.Manager.MainGame;
using UnityCore.UI.PlayWnd.Items;
using UnityEngine;

namespace UnityCore.UI.PlayWnd
{
    public partial class PlayWnd
    {
        [SerializeField] private List<SkillItem> skillItems;

        [SerializeField] private Transform imgInfoRoot;

        public void InitSkillInfo()
        {
            var gameManager = GameModuleManager.GetModule<MainGameManager>();
            if (gameManager == null)
            {
                GameDebug.LogError("MainGameManager is not initialized.");
                return;
            }
            
            var heroDataList = gameManager.HeroDataList;
            var playerIndex = gameManager.PlayerIndex;
            var hero = heroDataList[playerIndex];
            var cfg = Game.Config.Tables.TbUnit.Get(hero.HeroID);
            var skillList = cfg.Skills;

            int index = 0;
            foreach (var skillItem in skillItems)
            {
                skillItem.Index = index;
                var skillCfg = Game.Config.Tables.TbSkill.Get(hero.HeroID);
                skillItem.InitSkillItem(skillCfg, index);
            } 
        }
        
        private void SetForbidState(bool state){
            foreach (var skillItem in skillItems)
            {
                skillItem.SetForbidState(state);
            }
        }
    }
}