using System;
using UnityCore.GameModule.Battle.Logic;
using UnityEngine;

namespace UnityCore.GameModule.Battle.View
{
    public abstract class ViewUnit : MonoBehaviour
    {
        public bool IsSyncPos;
        [SerializeField] private int PredictMaxCount;
        [SerializeField] private bool PredictPos;
        [SerializeField] private bool SmoothPos;
        [SerializeField] private float viewPosAccer;

        public bool IsSyncDir;
        [SerializeField] private bool smoothDir;
        [SerializeField] private float angleMultiplier = 1f; // Used to control the smoothness of direction changes
        [SerializeField] private float viewDirAccer = 1f;

        [SerializeField] private Transform RotationRoot;

        private LogicUnit _logicUnit;
        private int _predictCount;
        
        protected Vector3 ViewTargetPos;
        protected Vector3 ViewTargetDir;

        public virtual void Init(LogicUnit logicUnit)
        {
            _logicUnit = logicUnit;
            var o = gameObject;
            o.name = logicUnit.UnitName + "_" + o.name;

            transform.position = logicUnit.LogicPosition;
            if (RotationRoot == null)
            {
                RotationRoot = transform;
            }

            RotationRoot.rotation = CalculateRotation(logicUnit.LogicDirection);
        }

        public void ForceSyncPosition()
        {
            transform.position = _logicUnit.LogicPosition;
        }

        protected virtual void Update()
        {
            if (IsSyncDir)
            {
                UpdateDirection();
            }

            if (IsSyncPos)
            {
                UpdatePosition();
            }
        }

        private void UpdateDirection()
        {
            if (_logicUnit.IsLogicDirDirty)
            {
                ViewTargetDir = GetUnitViewDirection();
                _logicUnit.IsLogicDirDirty = false;
            }
            
            if (smoothDir)
            {
                var threhold = Time.deltaTime * viewDirAccer;
                float angle = Vector3.Angle(RotationRoot.forward, ViewTargetDir);
                float angleMultiplierAdjusted = (angle / 180f) * angleMultiplier * Time.deltaTime;

                if (ViewTargetDir != Vector3.zero)
                {
                    Vector3 interDir = Vector3.Lerp(RotationRoot.forward, ViewTargetDir,
                        threhold + angleMultiplierAdjusted);
                    RotationRoot.rotation = CalculateRotation(interDir);
                }
            }
            else
            {
                RotationRoot.rotation = CalculateRotation(ViewTargetDir);
            }
        }

        private void UpdatePosition()
        {
            if (PredictPos)
            {
                if (_logicUnit.IsLogicPosDirty)
                {
                    ViewTargetPos = _logicUnit.LogicPosition;
                    _logicUnit.IsLogicPosDirty = false;
                    _predictCount = 0;
                }
                else
                {
                    if (_predictCount > PredictMaxCount)
                    {
                        return;
                    }

                    float deltaTime = Time.deltaTime;
                    var predictPos = deltaTime * _logicUnit.LogicSpeed.RawFloat * (Vector3)_logicUnit.LogicDirection;
                    ViewTargetPos += predictPos;
                    _predictCount++;
                }

                if (SmoothPos)
                {
                    transform.position = Vector3.Lerp(transform.position, ViewTargetPos, viewPosAccer * Time.deltaTime);
                }
                else
                {
                    transform.position = ViewTargetPos;
                }
            }
            else
            {
                ForceSyncPosition();
            }
        }
        
        protected virtual Vector3 GetUnitViewDirection()
        {
            return _logicUnit.LogicDirection;
        }
        
        protected Quaternion CalculateRotation(Vector3 targetDir)
        {
            return Quaternion.FromToRotation(Vector3.forward, targetDir);
        }
        
        public abstract void PlayAnim(string animName);
    }
}