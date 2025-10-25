using System;

namespace UnityCore.GameModule.Battle.Board
{
    public abstract class LatKey<T> : IKey, IEquatable<LatKey<T>>, IComparable<LatKey<T>>{
        private readonly string _name; // name由用户分配
        private readonly int _id; // ID通常自动计算，ID相等即相等
        private int _modifiers; // 修饰符
    
        public string Name => _name;
        public int Id => _id;
        
        // 抽象类定义拆装箱接口
        public abstract T Unbox(in LatValue value);
        public abstract LatValue Box(T value);
        public abstract bool Equals(LatKey<T> other);
        public abstract int CompareTo(LatKey<T> other);
        
        public override int GetHashCode() {
            return _id;
        }
        public override string ToString() {
            return _name;    
        }
    }
}