namespace NR_AutoMachineTool.Utilities;

public class Tuple<T1, T2>
{
    public T1 Value1;

    public T2 Value2;

    public Tuple(T1 v1, T2 v2)
    {
        Value1 = v1;
        Value2 = v2;
    }
}

public class Tuple<T1, T2, T3>
{
    public T1 Value1;

    public T2 Value2;

    public T3 Value3;

    public Tuple(T1 v1, T2 v2, T3 v3)
    {
        Value1 = v1;
        Value2 = v2;
        Value3 = v3;
    }
}