using UnityCore.GameModule.Battle.Logic;
using UnityEngine;

namespace UnityCore.GameModule.Battle.View
{
    public abstract class MainViewUnit : ViewUnit
    {
        public Transform SkillRange;
        public float fade;
        public Animation ani;

        private float _aniMoveSpeedBase;
        private MainLogicUnit _mainLogicUnit;

        public override void Init(LogicUnit logicUnit)
        {
            base.Init(logicUnit);
            
            _mainLogicUnit = logicUnit as MainLogicUnit;
            _aniMoveSpeedBase = _mainLogicUnit.LogicSpeed.RawFloat;
        }

        protected override void Update()
        {
            if (_mainLogicUnit.IsLogicDirDirty)
            {
                if (((Vector3)_mainLogicUnit.LogicDirection).Equals(Vector3.zero))
                {
                    PlayAnim("free");
                }else
                {
                    PlayAnim("walk");
                }
            }
            
            base.Update();
        }

        public override void PlayAnim(string animName)
        {
            if (animName.Contains("walk"))
            {
                float moveRate = _mainLogicUnit.LogicSpeed.RawFloat / _aniMoveSpeedBase;
                ani[animName].speed = moveRate;
                ani.CrossFade(animName, fade / moveRate);
            }
            else
            {
                ani.CrossFade(animName, fade);
            }
        }

        public void SetAtkSkillRange(bool state, float range = 2.5f)
        {
            if (SkillRange)
            {
                range += _mainLogicUnit.LogicUnitData.UnitCfg.ColliderConfig.Radius.RawFloat;
                SkillRange.localScale = new Vector3(range / 2.5f, range / 2.5f, 1f);
                SkillRange.gameObject.SetActive(state);
            }
        }
    }
}