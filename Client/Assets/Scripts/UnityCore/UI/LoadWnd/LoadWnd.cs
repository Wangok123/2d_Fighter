using System.Collections.Generic;
using GameProtocol;
using LATLog;
using UnityCore.Base;
using UnityCore.EventDefine;
using UnityCore.EventSystem;
using UnityCore.GameModule.Loading;
using UnityCore.UI.Core;
using UnityEngine;
using UnityEngine.UI;

namespace UnityCore.UI.LoadWnd
{
    public class LoadWnd : UIFormLogic, ILoadingUI
    {
        [SerializeField] private RectTransform bluePlayerRoot;
        [SerializeField] private List<LoadPlayerItem> bluePlayerItems;
        [SerializeField] private RectTransform redPlayerRoot;
        [SerializeField] private List<LoadPlayerItem> redPlayerItems;
        
        private ResponseEventDefine.LoadResNotificationArgs _args;
        private BattleHeroDto[] _allHeroes;

        protected internal override void OnInit(object mData)
        {
            base.OnInit(mData);
            _args = mData as ResponseEventDefine.LoadResNotificationArgs;
            if (_args == null)
            {
                GameDebug.LogError("LoadWnd initialization failed: LoadResNotificationArgs is null.");
                return;
            }

            _allHeroes = _args.HeroDataArray;
            int heroCount = _allHeroes.Length / 2;
            // blue team
            for (int i = 0; i < bluePlayerItems.Count; i++)
            {
                if (i < heroCount)
                {
                    var hero = _allHeroes[i];
                    int heroID = hero.HeroId;
                    var config = Game.Config.Tables.TbUnit.Get(heroID);
                    
                    bluePlayerItems[i].SetData(hero.UserName, config.UnitName, $"{config.ResName}_load");
                    bluePlayerItems[i].gameObject.SetActive(true);
                    bluePlayerItems[i].UpdateProgress(0);
                }
                else
                {
                    bluePlayerItems[i].gameObject.SetActive(false);
                }
            }
            
            // red team
            for (int i = 0; i < redPlayerItems.Count; i++)
            {
                if (i < heroCount)
                {
                    var hero = _allHeroes[i + heroCount];
                    int heroID = hero.HeroId;
                    var config = Game.Config.Tables.TbUnit.Get(heroID);
                    
                    redPlayerItems[i].SetData(hero.UserName, config.UnitName, $"{config.ResName}_load");
                    redPlayerItems[i].gameObject.SetActive(true);
                    redPlayerItems[i].UpdateProgress(0);
                }
                else
                {
                    redPlayerItems[i].gameObject.SetActive(false);
                }
            }
            
            LayoutRebuilder.ForceRebuildLayoutImmediate(bluePlayerRoot);
            LayoutRebuilder.ForceRebuildLayoutImmediate(redPlayerRoot);
        }

        protected internal override void OnOpen(object mData)
        {
            base.OnOpen(mData);
            Game.Audio.PlaySingleSound("audios_load");
        }

        public void OnLoadingStarted()
        {
            UniEvent.AddListener<ResponseEventDefine.LoadProgressNotificationArgs>(OnUpdateProgress);
        }

        public void SetProgress(float progress)
        {
        }

        public void OnLoadingComplete()
        {
            UniEvent.RemoveListener<ResponseEventDefine.LoadProgressNotificationArgs>(OnUpdateProgress);
        }

        private void OnUpdateProgress(IEventMessage message)
        {
            if (message is not ResponseEventDefine.LoadProgressNotificationArgs args)
            {
                return;
            }

            int heroCount = _allHeroes.Length / 2;
            for (int i = 0; i < args.PercentList.Length; i++)
            {
                if (i < heroCount)
                {
                    bluePlayerItems[i].UpdateProgress(args.PercentList[i]);
                }
                else
                {
                    redPlayerItems[i - heroCount].UpdateProgress(args.PercentList[i]);
                }
            }
        }
    }
}