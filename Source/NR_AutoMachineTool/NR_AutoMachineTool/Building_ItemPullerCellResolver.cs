using System.Collections.Generic;
using NR_AutoMachineTool.Utilities;
using UnityEngine;
using Verse;

namespace NR_AutoMachineTool;

public class Building_ItemPullerCellResolver : IOutputCellResolver, IInputCellResolver
{
    private static readonly List<IntVec3> EmptyList = new List<IntVec3>();

    public Option<IntVec3> InputCell(IntVec3 cell, Map map, Rot4 rot)
    {
        return Ops.Option(cell + rot.Opposite.FacingCell);
    }

    public IEnumerable<IntVec3> InputZoneCells(IntVec3 cell, Map map, Rot4 rot)
    {
        return (from c in InputCell(cell, map, rot)
            select c.SlotGroupCells(map)).GetOrDefault(EmptyList);
    }

    public ModExtension_AutoMachineTool Parent { get; set; }

    public Color GetColor(IntVec3 cell, Map map, Rot4 rot, CellPattern cellPattern)
    {
        return cellPattern.ToColor();
    }

    public Option<IntVec3> OutputCell(IntVec3 cell, Map map, Rot4 rot)
    {
        return Ops.Option(cell + rot.FacingCell);
    }

    public IEnumerable<IntVec3> OutputZoneCells(IntVec3 cell, Map map, Rot4 rot)
    {
        return (from c in OutputCell(cell, map, rot)
            select c.SlotGroupCells(map)).GetOrDefault(EmptyList);
    }
}