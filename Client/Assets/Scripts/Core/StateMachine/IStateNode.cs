namespace Core.StateMachine
{
    public interface IStateNode
    {
        public void OnCreate(StateMachine machine);
        public void OnEnter();
        public void OnExit();
        public void OnUpdate();
    }
}