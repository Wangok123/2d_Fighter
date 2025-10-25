using System.Collections;
using System.IO;
using LATLog;
using Luban;
using UnityCore.Base;
using UnityEngine;
using YooAsset;

namespace Configs
{
    public class ConfigComponent : LatComponent
    {
        private LatCfg.Tables _tables;
        public LatCfg.Tables Tables => _tables;

        protected override void Awake()
        {
            base.Awake();

            StartCoroutine(WaitForYooAsset());
        }

        private IEnumerator WaitForYooAsset()
        {
            yield return new WaitUntil(() => Game.YooAsset != null && Game.YooAsset.IsInit);
            _tables = new LatCfg.Tables(file =>
                new ByteBuf(GetBytes(file)));
            
            IsInit = true;
        }

        private byte[] GetBytes(string file)
        {
            var handle = Game.YooAsset.LoadRawAsset(file);
            return handle.GetRawFileData();
        }
    }
}