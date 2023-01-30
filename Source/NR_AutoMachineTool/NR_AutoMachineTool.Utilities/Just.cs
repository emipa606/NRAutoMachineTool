namespace NR_AutoMachineTool.Utilities;

public class Just<T> : Option<T>
{
    public Just(T value)
        : base(value)
    {
    }
}