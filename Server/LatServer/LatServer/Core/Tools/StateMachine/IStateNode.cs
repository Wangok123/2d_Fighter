namespace LatServer.Core.Tools.StateMachine;

public interface IStateNode
{
    public void OnCreate(StateMachine machine);
    public void OnEnter();
    public void OnExit();
}