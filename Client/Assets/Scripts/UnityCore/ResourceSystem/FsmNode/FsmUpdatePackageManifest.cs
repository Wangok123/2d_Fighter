using System.Collections;
using Core.StateMachine;
using LATLog;
using UnityCore.EventDefine;
using UnityCore.Tools;
using YooAsset;

namespace UnityCore.ResourceSystem.FsmNode
{
    public class FsmUpdatePackageManifest : IStateNode
    {
        private StateMachine _machine;
        
        public void OnCreate(StateMachine machine)
        {
            _machine = machine;
        }

        public void OnEnter()
        {
            PatchEventDefine.PatchStepsChange.SendEventMessage("更新资源清单 !");
            CoroutineManager.Instance.StartCoroutine(UpdateManifest());
        }

        public void OnExit()
        {
        }

        public void OnUpdate()
        {
        }
        
        private IEnumerator UpdateManifest()
        {
            var packageName = (string)_machine.GetBlackboardValue("PackageName");
            var packageVersion = (string)_machine.GetBlackboardValue("PackageVersion");
            var package = YooAssets.GetPackage(packageName);
            var operation = package.UpdatePackageManifestAsync(packageVersion);
            yield return operation;

            if (operation.Status != EOperationStatus.Succeed)
            {
                GameDebug.LogWarning(operation.Error);
                PatchEventDefine.PackageManifestUpdateFailed.SendEventMessage();
                yield break;
            }
            else
            {
                _machine.ChangeState<FsmCreateDownloader>();
            }
        }
    }
}