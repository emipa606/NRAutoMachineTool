namespace NR_AutoMachineTool;

public static class ITargetCellResolverExtension
{
    public static int MaxRange(this ITargetCellResolver r)
    {
        return r.GetRange(r.MaxPowerForRange);
    }

    public static int MinRange(this ITargetCellResolver r)
    {
        return r.GetRange(r.MinPowerForRange);
    }
}