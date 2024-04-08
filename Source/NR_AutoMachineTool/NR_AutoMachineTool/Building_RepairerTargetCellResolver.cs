using System.Collections.Generic;
using Verse;

namespace NR_AutoMachineTool;

public class Building_RepairerTargetCellResolver : BaseTargetCellResolver
{
    public override int MinPowerForRange => Setting.repairerSetting.minSupplyPowerForRange;

    public override int MaxPowerForRange => Setting.repairerSetting.maxSupplyPowerForRange;

    public override bool NeedClearingCache => false;

    public override IEnumerable<IntVec3> GetRangeCells(IntVec3 pos, Map map, Rot4 rot, int range)
    {
        return GenRadial.RadialCellsAround(pos, range, true);
    }
}