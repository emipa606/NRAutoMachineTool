using System.Collections.Generic;
using NR_AutoMachineTool.Utilities;
using UnityEngine;
using Verse;

namespace NR_AutoMachineTool;

public class OutputCellResolver : IOutputCellResolver
{
    private static readonly List<IntVec3> EmptyList = new List<IntVec3>();

    public ModExtension_AutoMachineTool Parent { get; set; }

    public virtual Option<IntVec3> OutputCell(IntVec3 cell, Map map, Rot4 rot)
    {
        return Ops.Option(cell + rot.Opposite.FacingCell);
    }

    public virtual IEnumerable<IntVec3> OutputZoneCells(IntVec3 cell, Map map, Rot4 rot)
    {
        return (from c in OutputCell(cell, map, rot)
            select c.SlotGroupCells(map)).GetOrDefault(EmptyList);
    }

    public virtual Color GetColor(IntVec3 cell, Map map, Rot4 rot, CellPattern cellPattern)
    {
        return cellPattern.ToColor();
    }
}