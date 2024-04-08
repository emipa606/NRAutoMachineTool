namespace NR_AutoMachineTool.Utilities;

public class Tuple<T1, T2>(T1 v1, T2 v2)
{
    public readonly T1 Value1 = v1;

    public readonly T2 Value2 = v2;
}

public class Tuple<T1, T2, T3>(T1 v1, T2 v2, T3 v3)
{
    public T1 Value1 = v1;

    public T2 Value2 = v2;

    public T3 Value3 = v3;
}