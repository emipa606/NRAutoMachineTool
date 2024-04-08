using System.Collections.Generic;
using System.Linq;
using NR_AutoMachineTool.Utilities;
using UnityEngine;
using Verse;

namespace NR_AutoMachineTool;

public class Building_PlanterTargetCellResolver : BaseTargetCellResolver
{
    public override int MinPowerForRange => Setting.PlanterTier(Parent.tier).minSupplyPowerForRange;

    public override int MaxPowerForRange => Setting.PlanterTier(Parent.tier).maxSupplyPowerForRange;

    public override bool NeedClearingCache => true;

    public override IEnumerable<IntVec3> GetRangeCells(IntVec3 pos, Map map, Rot4 rot, int range)
    {
        return from c in GenRadial.RadialCellsAround(pos, range, true)
            where c.GetRoom(map) == pos.GetRoom(map)
            select c;
    }

    public override Color GetColor(IntVec3 cell, Map map, Rot4 rot, CellPattern cellPattern)
    {
        var color = base.GetColor(cell, map, rot, cellPattern);
        if (!cell.GetPlantable(map).HasValue)
        {
            return color;
        }

        color = Color.green;
        if (cellPattern == CellPattern.BlurprintMax)
        {
            color = color.A(0.5f);
        }

        return color;
    }
}