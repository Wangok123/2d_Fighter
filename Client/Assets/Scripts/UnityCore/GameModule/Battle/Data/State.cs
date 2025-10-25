using UnityCore.GameModule.Battle.Board;
using UnityCore.GameModule.Battle.Config;
using Wjybxx.BTree;

namespace UnityCore.GameModule.Battle.Data
{
    public class State
    {
        public StateCfg Cfg; // 状态配置
        // ...
        public int Level; // 等级
        public int Stack; // 叠层
        public int TimeLeft; // 剩余时间
        // ...
        public Blackboard Blackboard; // 动态数据
        public TaskEntry<Blackboard> task; // 状态关联的脚本
        public StateSlot slot; // 绑定的状态槽
        public bool active; // 是否处于活动状态
    }
}