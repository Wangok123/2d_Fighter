namespace UnityCore.GameModule.Battle.Board
{
    public enum ValueType : byte {
        Undefine = 0, // 表示Key不存在
        Null = 1, // 表示Key存在，但值为null
        Int = 2,
        Long = 3,
        Float = 4,
        Double = 5,
        Bool = 6,
        Vector3 = 7,
        // ...
        Object = 15, // 引用类型
    }
}