namespace LATMath;

public struct LATArgs
{
    public int Value;
    public uint Multiplier;

    public LATArgs(int value, uint multiplier)
    {
        Value = value;
        Multiplier = multiplier;
    }

    public static LATArgs Zero => new LATArgs(0, 10000);
    public static LATArgs HalfPi => new LATArgs(15708, 10000);
    public static LATArgs Pi => new LATArgs(31416, 10000);
    public static LATArgs TwoPi => new LATArgs(62832, 10000);

    public static bool operator ==(LATArgs a, LATArgs b)
    {
        if (a.Multiplier == b.Multiplier)
        {
            return a.Value == b.Value;
        }
        else
        {
            throw new System.Exception("LATArgs can't compare with different Multiplier");
        }
    }
    
    public static bool operator !=(LATArgs a, LATArgs b)
    {
        if (a.Multiplier == b.Multiplier)
        {
            return a.Value != b.Value;
        }
        else
        {
            throw new System.Exception("LATArgs can't compare with different Multiplier");
        }
    }
    
    public static bool operator >(LATArgs a, LATArgs b)
    {
        if (a.Multiplier == b.Multiplier)
        {
            return a.Value > b.Value;
        }
        else
        {
            throw new System.Exception("LATArgs can't compare with different Multiplier");
        }
    }
    
    public static bool operator <(LATArgs a, LATArgs b)
    {
        if (a.Multiplier == b.Multiplier)
        {
            return a.Value < b.Value;
        }
        else
        {
            throw new System.Exception("LATArgs can't compare with different Multiplier");
        }
    }
    
    public static bool operator >=(LATArgs a, LATArgs b)
    {
        if (a.Multiplier == b.Multiplier)
        {
            return a.Value >= b.Value;
        }
        else
        {
            throw new System.Exception("LATArgs can't compare with different Multiplier");
        }
    }
    
    public static bool operator <=(LATArgs a, LATArgs b)
    {
        if (a.Multiplier == b.Multiplier)
        {
            return a.Value <= b.Value;
        }
        else
        {
            throw new System.Exception("LATArgs can't compare with different Multiplier");
        }
    }
    
    /// <summary>
    /// 转化为视图角度，不可用于计算
    /// </summary>
    /// <returns></returns>
    public int ConvertToAngle()
    {
        float rad = ConvertToRad();
        return (int)(rad * 180 / Math.PI);
    }
    
    /// <summary>
    /// 转化为视图弧度，不可用于计算
    /// </summary>
    /// <returns></returns>
    public float ConvertToRad()
    {
        return (float)Value / Multiplier;
    }

    public override bool Equals(object obj)
    {
        return obj is LATArgs args && args.Value == Value && args.Multiplier == Multiplier;
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode() ^ Multiplier.GetHashCode();
    }

    public override string ToString()
    {
        return $"value:{Value},{Multiplier}";
    }
}