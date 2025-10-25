using System.Collections;
using Core.StateMachine;
using LATLog;
using UnityCore.EventDefine;
using UnityCore.Tools;
using YooAsset;

namespace UnityCore.ResourceSystem.FsmNode
{
    public class FsmRequestPackageVersion : IStateNode
    {
        private StateMachine _machine;
        
        public void OnCreate(StateMachine machine)
        {
            _machine = machine;
        }

        public void OnEnter()
        {
            PatchEventDefine.PatchStepsChange.SendEventMessage("请求资源版本 !");
            CoroutineManager.Instance.StartCoroutine(UpdatePackageVersion());
        }

        public void OnExit()
        {
        }

        public void OnUpdate()
        {
        }
        
        private IEnumerator UpdatePackageVersion()
        {
            var packageName = (string)_machine.GetBlackboardValue("PackageName");
            var package = YooAssets.GetPackage(packageName);
            var operation = package.RequestPackageVersionAsync();
            yield return operation;

            if (operation.Status != EOperationStatus.Succeed)
            {
                GameDebug.LogWarning(operation.Error);
                PatchEventDefine.PackageVersionRequestFailed.SendEventMessage();
            }
            else
            {
                GameDebug.Log($"Request package version : {operation.PackageVersion}");
                _machine.SetBlackboardValue("PackageVersion", operation.PackageVersion);
                _machine.ChangeState<FsmUpdatePackageManifest>();
            }
        }
    }
}