using System;
using System.Collections;
using Configs;
using LatCfg.latcfg;
using UnityCore.Base;
using UnityCore.Extensions.UI;
using UnityCore.GameModule.Battle.Manager.MainGame;
using UnityCore.GameModule.Battle.View;
using UnityEngine;
using UnityEngine.UI;

namespace UnityCore.UI.PlayWnd.Items
{
    public class SkillItem : MonoBehaviour
    {
        [SerializeField] private LatButton btnSkill;
        [SerializeField] private LatImage imgCircle;
        [SerializeField] private LatImage skillIcon;
        [SerializeField] private LatImage imgCD;
        [SerializeField] private LatImage imgPoint;
        [SerializeField] private LatImage imgForbid;
        [SerializeField] private Transform EffectRoot;
        [SerializeField] private Text textCD;
        public int Index { get; set; }

        private MainGameManager _manager;
        
        private HeroView viewHero;
        private int _skillIndex;
        private Skill _skillCfg;
        private float _pointDis;

        private void Awake()
        {
            _manager = GameModuleManager.GetModule<MainGameManager>();
        }

        private void OnEnable()
        {
            btnSkill.onClick.AddListener(OnClick);
            btnSkill.OnUp.AddListener(OnUp);
        }
        
        private void OnDisable()
        {
            btnSkill.onClick.RemoveListener(OnClick);
            btnSkill.OnUp.RemoveListener(OnUp);
        }

        private void OnDestroy()
        {
            if (ct != null)
            {
                StopCoroutine(ct);
                ct = null;
            }
        }

        public void InitSkillItem(Skill skillCfg, int skillIndex)
        {
            EffectRoot.gameObject.SetActive(false);
            
            viewHero = _manager.GetSelfHero().ViewUnit as HeroView;
            
            _skillIndex = skillIndex;
            _skillCfg = skillCfg;
            
            _pointDis = Screen.height * 1.0f / ClientConfig.ScreenStandardHeight * ClientConfig.SkillOPDis;
        }

        public void SetForbidState(bool state)
        {
            imgForbid.gameObject.SetActive(state);
        }
        
        private void OnClick()
        {
            if (_skillCfg.IsNormalAttack)
            {
                ShowSkillAtkRange(true);
                ClickSkillItem();
            }
            else
            {
                
            }
        }

        private void OnUp()
        {
            if (_skillCfg.IsNormalAttack)
            {
                ShowSkillAtkRange(false);
                ShowEffect();
            }
            else
            {
                
            }
        }

        private void ClickSkillItem()
        {
            _manager.SendSkillKey(_skillCfg.Id, Vector3.zero);
        }

        private void ClickSkillItem(Vector3 vec)
        {
            _manager.SendSkillKey(_skillCfg.Id, vec);
        }

        private Coroutine ct;
        private void ShowEffect()
        {
            if (ct != null)
            {
                StopCoroutine(ct);
                EffectRoot.gameObject.SetActive(false);
            }

            EffectRoot.gameObject.SetActive(true);
            ct = StartCoroutine(DisableEffect());
        }

        private IEnumerator DisableEffect()
        {
            yield return new WaitForSeconds(0.5f);
            EffectRoot.gameObject.SetActive(false);
        }

        private void ShowSkillAtkRange(bool active)
        {
            if (_skillCfg.TargetCfg > 0)
            {
                // var targetCfg = Game.Config.Tables.Tb
                // viewHero.SetAtkSkillRange(active, _skillCfg.TargetCfg);
            }
        }
    }
}