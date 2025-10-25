using System;
using System.Runtime.InteropServices;
using UnityEngine;
using ValueType = UnityCore.GameModule.Battle.Board.ValueType;

namespace UnityCore.GameModule.Battle.Board
{
    
    [StructLayout(LayoutKind.Explicit)]
    public struct LatValue: IEquatable<LatValue>
    {
        [FieldOffset(0)] public readonly ValueType type;
        [FieldOffset(1)] public int intVal;
        [FieldOffset(1)] public long longVal;
        [FieldOffset(1)] public float floatVal;    
        [FieldOffset(1)] public double doubleVal;
        [FieldOffset(1)] public bool boolVal;
        [FieldOffset(1)] public Vector3 vector3; // sizeof => 12
        // ...
        [FieldOffset(16)] public object objectVal; // 内存对齐后需要偏移16
        public bool Equals(LatValue other)
        {
            if (type != other.type) return false;
            switch (type)
            {
                case ValueType.Int:
                    return intVal == other.intVal;
                case ValueType.Long:
                    return longVal == other.longVal;
                case ValueType.Float:
                    return Math.Abs(floatVal - other.floatVal) < 1e-6f;
                case ValueType.Double:
                    return Math.Abs(doubleVal - other.doubleVal) < 1e-6;
                case ValueType.Bool:
                    return boolVal == other.boolVal;
                case ValueType.Vector3:
                    return vector3.Equals(other.vector3);
                case ValueType.Object:
                    return object.Equals(objectVal, other.objectVal);
                default:
                    return true; // Undefine or Null
            }
        }
    }
}