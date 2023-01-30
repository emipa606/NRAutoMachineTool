using System.Collections.Generic;
using NR_AutoMachineTool.Utilities;
using UnityEngine;
using Verse;

namespace NR_AutoMachineTool;

public interface IOutputCellResolver
{
    ModExtension_AutoMachineTool Parent { get; set; }

    Option<IntVec3> OutputCell(IntVec3 cell, Map map, Rot4 rot);

    IEnumerable<IntVec3> OutputZoneCells(IntVec3 cell, Map map, Rot4 rot);

    Color GetColor(IntVec3 cell, Map map, Rot4 rot, CellPattern cellPattern);
}