namespace LATMath;

/// <summary>
/// 常用定点数计算
/// </summary>
public class LATCalculate
{
    public static LATInt Sqrt(LATInt value, int interator = 8)
    {
        if (value == 0)
        {
            return 0;
        }
        
        if (value < 0)
        {
            throw new ArgumentOutOfRangeException("value", "LATInt.Sqrt can't be used on a negative number");
        }
        LATInt result = value;
        LATInt history;
        int count = interator;
        do
        {
            history = result;
            result = (result + value / result) >> 1;
            count--;
        } while (result != history && count > 0);
        
        return result;
    }
    
    public static LATArgs Acos(LATInt value)
    {
        LATInt rate = (value * AcosTable.HalfIndexCount) + AcosTable.HalfIndexCount;
        rate = Clamp(rate, 0, AcosTable.IndexCount - 1);
        int rad = AcosTable.Table[rate.RawInt];

        return new LATArgs(rad, AcosTable.Multiplier);
    }
    
    public static LATInt Clamp(LATInt value, LATInt min, LATInt max)
    {
        if (value < min)
        {
            return min;
        }
        if (value > max)
        {
            return max;
        }
        return value;
    }
}