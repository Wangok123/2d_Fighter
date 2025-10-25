using System.Collections.Generic;
using LATLog;
using LATMath;
using LATPhysics;
using UnityCore.GameModule.Battle.GamePhysics;
using UnityEngine;
using NotImplementedException = System.NotImplementedException;

namespace UnityCore.GameModule.Battle.Manager.MainGame
{
    public class PhysicEnvManager
    {
        private EnvColliders _logicEnv;
        private MapRoot _mapRoot;
        
        public void Init(MapRoot mapRoot)
        {
            _mapRoot = mapRoot;
            var envColliderConfigs = GenerateEnvColliderConfigs(_mapRoot.transform);
            _logicEnv = new EnvColliders()
            {
                EnvColliderConfigs = envColliderConfigs
            };
            
            _logicEnv.Init();
        }
        
        public List<LATColliderBase> GetEnvColliders()
        {
            if (_logicEnv == null)
            {
                GameDebug.LogError("PhysicEnvManager is not initialized.");
                return new List<LATColliderBase>();
            }
            return _logicEnv.GetEnvColliders();
        }

        private List<ColliderConfig> GenerateEnvColliderConfigs(Transform transEnvRoot)
        {
            List<ColliderConfig> envColliderConfigs = new List<ColliderConfig>();
            BoxCollider[] boxArr = transEnvRoot.GetComponentsInChildren<BoxCollider>();
            for (int i = 0; i < boxArr.Length; i++)
            {
                Transform trans = boxArr[i].transform;
                var cfg = new ColliderConfig()
                {
                    Position = new LATVector3(trans.position)
                };

                cfg.Size = new LATVector3(trans.localScale / 2);
                cfg.ColliderType = ColliderType.Box;
                cfg.Axis = new LATVector3[3];
                cfg.Axis[0] = new LATVector3(trans.right);
                cfg.Axis[1] = new LATVector3(trans.up);
                cfg.Axis[2] = new LATVector3(trans.forward);
                
                envColliderConfigs.Add(cfg);
            }
            
            CapsuleCollider[] capsuleArr = transEnvRoot.GetComponentsInChildren<CapsuleCollider>();
            for (int i = 0; i < capsuleArr.Length; i++)
            {
                Transform trans = capsuleArr[i].transform;
                var cfg = new ColliderConfig()
                {
                    Position = new LATVector3(trans.position)
                };
                
                cfg.ColliderType = ColliderType.Circle;
                cfg.Radius = (LATInt)(trans.localScale.x / 2); // 假设半径是x轴的半径
                
                envColliderConfigs.Add(cfg);
            }
            
            return envColliderConfigs;
        }

        public void UnInit()
        {
            throw new NotImplementedException();
        }
    }
}