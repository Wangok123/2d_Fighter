using Core.StateMachine;
using UnityCore.EventDefine;

namespace UnityCore.ResourceSystem.FsmNode
{
    public class FsmDownloadPackageOver : IStateNode
    {
        private StateMachine _machine;

        void IStateNode.OnCreate(StateMachine machine)
        {
            _machine = machine;
        }
        void IStateNode.OnEnter()
        {
            PatchEventDefine.PatchStepsChange.SendEventMessage("资源文件下载完毕！");
            _machine.ChangeState<FsmClearCacheBundle>();
        }
        void IStateNode.OnUpdate()
        {
        }
        void IStateNode.OnExit()
        {
        }
    }
}