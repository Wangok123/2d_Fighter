using Core.StateMachine;
using UnityCore.EventDefine;

namespace UnityCore.ResourceSystem.FsmNode
{
    public class FsmStartGame : IStateNode
    {
        private PatchOperation _owner;

        void IStateNode.OnCreate(StateMachine machine)
        {
            _owner = machine.Owner as PatchOperation;
        }
        void IStateNode.OnEnter()
        {
            PatchEventDefine.PatchStepsChange.SendEventMessage("开始游戏！");
            _owner.SetFinish();
        }
        void IStateNode.OnUpdate()
        {
        }
        void IStateNode.OnExit()
        {
        }
    }
}