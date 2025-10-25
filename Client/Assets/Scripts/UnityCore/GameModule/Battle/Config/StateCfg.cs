using System.Collections.Generic;

namespace UnityCore.GameModule.Battle.Config
{
    public class StateCfg
    {
        public int ID; // 配置id
        public int Category; // 种类：技能、Buff、其它
        public List<int> TypeTags; // 类型标签，buff、debuff、出血、冰冻、破防...
        public List<int> FuncTags; // 功能标签，禁止移动，禁止施法...
        // ...
        public int Level; // 默认等级
        public int Duration; // 持续时间    
        public int Stack; // 默认叠层
        public int MaxStack; // 最大叠加数
        // 互斥
        public int Slot; // 绑定的静态槽，大于0有效
        public List<int> GraphStateIds; // 互斥图重定向
        public int SameCasterPolicy; // 同施法者之间的互斥策略，丢弃旧的、丢弃新的、叠层刷新...
        public int DiffCasterPolicy; // 不同施法者之间的互斥策略，并行、丢弃旧的、丢弃新的...
        // ...
        public bool AllowDispel; // 允许驱散
        public bool AlloPurify; // 允许净化
        // ...
        public string TaskName; // 脚本名字--我用的是行为树
    }
}