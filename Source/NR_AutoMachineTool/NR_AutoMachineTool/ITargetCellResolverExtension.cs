namespace NR_AutoMachineTool;

public static class ITargetCellResolverExtension
{
    extension(ITargetCellResolver r)
    {
        public int MaxRange()
        {
            return r.GetRange(r.MaxPowerForRange);
        }

        public int MinRange()
        {
            return r.GetRange(r.MinPowerForRange);
        }
    }
}