using LatServer.Core.Tools.StateMachine;

namespace LatServer.Core.System.RoomSys.Fsm;

public class RoomStateEnd : IStateNode
{
    private StateMachine _machine;
    
    public void OnCreate(StateMachine machine)
    {
        _machine = machine;
    }

    public void OnEnter()
    {
    }

    public void OnExit()
    {
    }
    
}