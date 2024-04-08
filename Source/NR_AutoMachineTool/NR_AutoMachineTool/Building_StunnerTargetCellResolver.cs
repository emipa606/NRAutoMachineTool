using System.Collections.Generic;
using Verse;

namespace NR_AutoMachineTool;

public class Building_StunnerTargetCellResolver : BaseTargetCellResolver
{
    public override int MinPowerForRange => Setting.stunnerSetting.minSupplyPowerForRange;

    public override int MaxPowerForRange => Setting.stunnerSetting.maxSupplyPowerForRange;

    public override bool NeedClearingCache => false;

    public override IEnumerable<IntVec3> GetRangeCells(IntVec3 pos, Map map, Rot4 rot, int range)
    {
        return GenRadial.RadialCellsAround(pos, range, true);
    }
}