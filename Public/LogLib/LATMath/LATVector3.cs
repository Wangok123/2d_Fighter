#if UNITY_ENV
using UnityEngine;
#endif

namespace LATMath
{
    public struct LATVector3
    {
        public LATInt x;
        public LATInt y;
        public LATInt z;

#if UNITY_ENV
        public LATVector3(Vector3 v)
        {
            x = (LATInt)v.x;
            y = (LATInt)v.y;
            z = (LATInt)v.z;
        }
#endif

        public LATVector3(LATInt x, LATInt y, LATInt z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public LATInt this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0:
                        return x;
                    case 1:
                        return y;
                    case 2:
                        return z;
                    default:
                        throw new IndexOutOfRangeException("Invalid LATVector3 index!");
                }
            }
            set
            {
                switch (index)
                {
                    case 0:
                        x = value;
                        break;
                    case 1:
                        y = value;
                        break;
                    case 2:
                        z = value;
                        break;
                    default:
                        throw new IndexOutOfRangeException("Invalid LATVector3 index!");
                }
            }
        }

        public static LATVector3 Zero => new LATVector3(0, 0, 0);
        public static LATVector3 One => new LATVector3(1, 1, 1);
        public static LATVector3 Forward => new LATVector3(0, 0, 1);
        public static LATVector3 Back => new LATVector3(0, 0, -1);
        public static LATVector3 Up => new LATVector3(0, 1, 0);
        public static LATVector3 Down => new LATVector3(0, -1, 0);
        public static LATVector3 Right => new LATVector3(1, 0, 0);
        public static LATVector3 Left => new LATVector3(-1, 0, 0);

        public static LATVector3 operator +(LATVector3 a, LATVector3 b)
        {
            return new LATVector3(a.x + b.x, a.y + b.y, a.z + b.z);
        }

        public static LATVector3 operator -(LATVector3 a, LATVector3 b)
        {
            return new LATVector3(a.x - b.x, a.y - b.y, a.z - b.z);
        }

        public static LATVector3 operator *(LATVector3 a, LATInt d)
        {
            return new LATVector3(a.x * d, a.y * d, a.z * d);
        }

        public static LATVector3 operator *(LATInt d, LATVector3 a)
        {
            return new LATVector3(a.x * d, a.y * d, a.z * d);
        }

        public static LATVector3 operator /(LATVector3 a, LATInt d)
        {
            return new LATVector3(a.x / d, a.y / d, a.z / d);
        }

        public static bool operator ==(LATVector3 a, LATVector3 b)
        {
            return a.x == b.x && a.y == b.y && a.z == b.z;
        }

        public static bool operator !=(LATVector3 a, LATVector3 b)
        {
            return a.x != b.x || a.y != b.y || a.z != b.z;
        }

        public static LATVector3 operator -(LATVector3 a)
        {
            return new LATVector3(-a.x, -a.y, -a.z);
        }

        /// <summary>
        /// 当前向量的模的平方
        /// </summary>
        public LATInt sqrMagnitude => x * x + y * y + z * z;

        public static LATInt SqrMagnitude(LATVector3 v)
        {
            return v.x * v.x + v.y * v.y + v.z * v.z;
        }

        public LATInt Magnitude => LATCalculate.Sqrt(sqrMagnitude);

        public LATVector3 Normalized
        {
            get
            {
                if (Magnitude > 0)
                {
                    LATInt rate = LATInt.One / Magnitude;
                    return new LATVector3(x * rate, y * rate, z * rate);
                }
                else
                {
                    return Zero;
                }
            }
        }

        public static LATVector3 Normalize(LATVector3 v)
        {
            if (v.Magnitude > 0)
            {
                LATInt rate = LATInt.One / v.Magnitude;
                return new LATVector3(v.x * rate, v.y * rate, v.z * rate);
            }
            else
            {
                return Zero;
            }
        }

        public void Normalize()
        {
            LATInt rate = LATInt.One / Magnitude;
            x *= rate;
            y *= rate;
            z *= rate;
        }
        
        /// <summary>
        /// 点乘
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static LATInt Dot(LATVector3 a, LATVector3 b)
        {
            return a.x * b.x + a.y * b.y + a.z * b.z;
        }
        
        /// <summary>
        /// 叉乘
        /// </summary>
        public static LATVector3 Cross(LATVector3 lhs, LATVector3 rhs)
        {
            return new LATVector3(
                lhs.y * rhs.z - lhs.z * rhs.y,
                lhs.z * rhs.x - lhs.x * rhs.z,
                lhs.x * rhs.y - lhs.y * rhs.x
            );
        }
        
        public static LATArgs Angle(LATVector3 from, LATVector3 to)
        {
            LATInt dot = Dot(from, to);
            LATInt m = from.Magnitude * to.Magnitude;
            if (m == 0)
            {
                return LATArgs.Zero;
            }
            
            LATInt value = dot / m;
            // 计算反余弦
            
            return LATCalculate.Acos(value);
        }

#if UNITY_ENV
        /// <summary>
        ///  仅用于显示，不要用于计算
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public static implicit operator Vector3(LATVector3 v)
        {
            return new Vector3(v.x.RawFloat, v.y.RawFloat, v.z.RawFloat);
        }

        /// <summary>
        /// 浮点型，不可用于计算
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public static Vector3 ConvertToVector3(LATVector3 v)
        {
            return new Vector3(v.x.RawFloat, v.y.RawFloat, v.z.RawFloat);
        }
#endif

        public long[] ConvertToLongArray()
        {
            return new long[] { x.Value, y.Value, z.Value };
        }

        public override bool Equals(object obj)
        {
            if (obj is LATVector3 v)
            {
                return x == v.x && y == v.y && z == v.z;
            }

            return false;
        }

        public override string ToString()
        {
            return $"({x}, {y}, {z})";
        }

        public override int GetHashCode()
        {
            return x.GetHashCode() ^ y.GetHashCode() ^ z.GetHashCode();
        }
    }
}