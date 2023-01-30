using System.Collections.Generic;
using NR_AutoMachineTool.Utilities;
using UnityEngine;
using Verse;

namespace NR_AutoMachineTool;

public class Building_AutoMachineToolCellResolver : BaseTargetCellResolver, IOutputCellResolver
{
    private static readonly List<IntVec3> EmptyList = new List<IntVec3>();

    public override int MinPowerForRange => Setting.AutoMachineToolTier(Parent.tier).minSupplyPowerForRange;

    public override int MaxPowerForRange => Setting.AutoMachineToolTier(Parent.tier).maxSupplyPowerForRange;

    public override bool NeedClearingCache => false;

    public Option<IntVec3> OutputCell(IntVec3 cell, Map map, Rot4 rot)
    {
        return from b in cell.GetThingList(map).SelectMany(b => Ops.Option(b as Building_AutoMachineTool)).FirstOption()
            select b.OutputCell();
    }

    public IEnumerable<IntVec3> OutputZoneCells(IntVec3 cell, Map map, Rot4 rot)
    {
        return (from c in OutputCell(cell, map, rot)
            select c.SlotGroupCells(map)).GetOrDefault(EmptyList);
    }

    public override IEnumerable<IntVec3> GetRangeCells(IntVec3 pos, Map map, Rot4 rot, int range)
    {
        return GenAdj.CellsOccupiedBy(pos, rot, new IntVec2(1, 1) + new IntVec2(range * 2, range * 2));
    }

    public override int GetRange(float power)
    {
        return Mathf.RoundToInt(power / 500f) + 1;
    }
}