using System.Collections.Generic;
using System.Linq;
using NR_AutoMachineTool.Utilities;
using UnityEngine;
using Verse;

namespace NR_AutoMachineTool;

public class Building_SlaughterhouseTargetCellResolver : BaseTargetCellResolver
{
    public override int MinPowerForRange => Setting.slaughterSetting.minSupplyPowerForRange;

    public override int MaxPowerForRange => Setting.slaughterSetting.maxSupplyPowerForRange;

    public override bool NeedClearingCache => true;

    public override IEnumerable<IntVec3> GetRangeCells(IntVec3 pos, Map map, Rot4 rot, int range)
    {
        return from c in Ops.FacingRect(pos, rot, range)
            where (pos + rot.FacingCell).GetRoom(map) == c.GetRoom(map)
            select c;
    }

    public override int GetRange(float power)
    {
        return Mathf.RoundToInt(power / 500f) + 1;
    }
}