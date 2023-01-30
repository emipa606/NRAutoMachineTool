using System.Collections.Generic;
using System.Linq;
using Verse;

namespace NR_AutoMachineTool;

public class Building_CleanerTargetCellResolver : BaseTargetCellResolver
{
    public override int MinPowerForRange => Setting.cleanerSetting.minSupplyPowerForRange;

    public override int MaxPowerForRange => Setting.cleanerSetting.maxSupplyPowerForRange;

    public override bool NeedClearingCache => true;

    public override IEnumerable<IntVec3> GetRangeCells(IntVec3 pos, Map map, Rot4 rot, int range)
    {
        return from c in GenRadial.RadialCellsAround(pos, range, true)
            where c.GetRoom(Find.CurrentMap) == pos.GetRoom(map)
            select c;
    }
}