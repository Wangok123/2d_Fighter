using LatServer.Core.Tools.StateMachine;

namespace LatServer.Core.System.RoomSys.Fsm;

public class RoomStateFighting : IStateNode
{
    private StateMachine _machine;
    
    public void OnCreate(StateMachine machine)
    {
        _machine = machine;
    }

    public void OnEnter()
    {
        throw new NotImplementedException();
    }

    public void OnExit()
    {
        throw new NotImplementedException();
    }

    public void OnUpdate()
    {
        throw new NotImplementedException();
    }
}