using System.Collections.Generic;
using System.Linq;
using LATLog;
using UniRx;
using UnityCore.Base;
using UnityCore.Entities.User;
using UnityCore.Extensions.UI;
using UnityCore.GameModule.HeroSelect;
using UnityCore.UI.Core;
using UnityEngine;

namespace UnityCore.UI.SelectWnd
{
    public class
        SelectWnd : UIFormLogic
    {
        [SerializeField] private HeroSelectScroller scroller;
        [SerializeField] private List<LatImage> skillIcons;
        [SerializeField] private LatRawImage heroMainShowImage;
        [SerializeField] private LatButton confirmButton;

        private const string AtlasName = "SkillIcon";
        private int _currentHeroId = -1;

        protected internal override void OnInit(object mData)
        {
            confirmButton.onClick.AsObservable()
                .Subscribe(_ => OnAcceptClicked())
                .AddTo(this);

            base.OnInit(mData);

            var heroConfig = Game.Config.Tables.TbUnit;
            if (heroConfig == null)
            {
                GameDebug.LogError("SelectWnd: Hero config is null");
                return;
            }

            var userData = Game.World.GetSingletonEntity<UserDataEntity>();
            var heroSelectDataComponent = userData.GetComponent<HeroDataComponent>();
            var list = heroSelectDataComponent.GetHeroSelectDataList();

            _currentHeroId = list.FirstOrDefault()?.HeroId ?? -1;

            scroller.SetData(list);
            scroller.OnSelectAction = OnSelectClicked;
        }

        protected internal override void OnOpen(object mData)
        {
            base.OnOpen(mData);

            scroller.Refresh();
            scroller.JumpTo(0, 0, () => { scroller.SelectDefaultHero(_currentHeroId); });
        }

        private void OnSelectClicked(int heroId)
        {
            _currentHeroId = heroId;

            var heroCfg = Game.Config.Tables.TbUnit.Get(heroId);
            heroMainShowImage.LoadTexture($"{heroCfg.ResName}_show");
            for (int i = 0; i < skillIcons.Count; i++)
            {
                skillIcons[i].LoadImage(AtlasName, $"{heroCfg.ResName}_sk{i}");
            }
        }

        private void OnAcceptClicked()
        {
            var selectManager = GameModuleManager.GetModule<HeroSelectManager>();
            selectManager.ConfirmHeroSelect(_currentHeroId);
            
            confirmButton.interactable = false;
        }
    }
}