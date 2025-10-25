namespace LATMath
{
    public struct LATInt
    {
        private const int BitMoveCount = 10;
        private const long MultiplierFactor = 1 << BitMoveCount;

        private long _scaledValue;

        public long Value
        {
            get => _scaledValue;
            set => _scaledValue = value;
        }

        public LATInt(int val)
        {
            _scaledValue = val * MultiplierFactor;
        }

        public LATInt(float val)
        {
            _scaledValue = (long)Math.Round(val * MultiplierFactor);
        }

        private LATInt(long scaledValue)
        {
            _scaledValue = scaledValue;
        }

        /// <summary>
        /// 损失精度，必须显示转换
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public static explicit operator LATInt(float val)
        {
            return new LATInt((long)Math.Round(val * MultiplierFactor));
        }

        public static implicit operator LATInt(int val)
        {
            return new LATInt(val);
        }

        public static LATInt operator +(LATInt a, LATInt b)
        {
            return new LATInt(a._scaledValue + b._scaledValue);
        }

        public static LATInt operator -(LATInt a, LATInt b)
        {
            return new LATInt(a._scaledValue - b._scaledValue);
        }

        public static LATInt operator *(LATInt a, LATInt b)
        {
            long value = a._scaledValue * b._scaledValue;
            if (value >= 0)
            {
                value >>= BitMoveCount;
            }
            else
            {
                value = -(-value >> BitMoveCount);
            }

            return new LATInt(value);
        }

        public static LATInt operator /(LATInt a, LATInt b)
        {
            if (b._scaledValue == 0)
            {
                throw new System.DivideByZeroException();
            }

            return new LATInt((a._scaledValue << BitMoveCount) / b._scaledValue);
        }

        public static LATInt operator -(LATInt val)
        {
            return new LATInt(-val._scaledValue);
        }

        public static bool operator ==(LATInt a, LATInt b)
        {
            return a._scaledValue == b._scaledValue;
        }

        public static bool operator !=(LATInt a, LATInt b)
        {
            return a._scaledValue != b._scaledValue;
        }

        public static bool operator >(LATInt a, LATInt b)
        {
            return a._scaledValue > b._scaledValue;
        }

        public static bool operator <(LATInt a, LATInt b)
        {
            return a._scaledValue < b._scaledValue;
        }

        public static bool operator >=(LATInt a, LATInt b)
        {
            return a._scaledValue >= b._scaledValue;
        }

        public static bool operator <=(LATInt a, LATInt b)
        {
            return a._scaledValue <= b._scaledValue;
        }

        public static LATInt operator >> (LATInt value, int moveCount)
        {
            if (value._scaledValue >= 0)
            {
                return new LATInt(value._scaledValue >> moveCount);
            }
            else
            {
                return new LATInt(-(-value._scaledValue >> moveCount));
            }
        }

        public static LATInt operator <<(LATInt value, int moveCount)
        {
            return new LATInt(value._scaledValue << moveCount);
        }

        /// <summary>
        /// 仅用于显示，不要用于计算
        /// </summary>
        public float RawFloat => _scaledValue * 1.0f / MultiplierFactor;

        /// <summary>
        ///  仅用于显示，不要用于计算
        /// </summary>
        public int RawInt
        {
            get
            {
                if (_scaledValue >= 0)
                {
                    return (int)(_scaledValue >> BitMoveCount);
                }
                else
                {
                    return -(int)(-_scaledValue >> BitMoveCount);
                }
            }
        }

        public static LATInt Zero => new LATInt(0);
        public static LATInt One => new LATInt(1);

        public override string ToString()
        {
            return RawFloat.ToString();
        }

        public override bool Equals(object? obj)
        {
            if (obj == null)
                return false;

            LATInt other = (LATInt)obj;
            return _scaledValue == other._scaledValue;
        }

        public override int GetHashCode()
        {
            return _scaledValue.GetHashCode();
        }
    }
}