namespace UnityCore.GameModule.Battle.Data
{
    public class StateSlot
    {
        public int ID; // 状态槽编号
        public State state; // 状态槽上的数据
    
        // 是否是静态槽，常量配置在外部
        public bool IsStatic => ID <= 5;
    }
}