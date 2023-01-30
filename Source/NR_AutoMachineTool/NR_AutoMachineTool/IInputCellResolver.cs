using System.Collections.Generic;
using NR_AutoMachineTool.Utilities;
using UnityEngine;
using Verse;

namespace NR_AutoMachineTool;

public interface IInputCellResolver
{
    ModExtension_AutoMachineTool Parent { get; set; }

    Option<IntVec3> InputCell(IntVec3 cell, Map map, Rot4 rot);

    IEnumerable<IntVec3> InputZoneCells(IntVec3 cell, Map map, Rot4 rot);

    Color GetColor(IntVec3 cell, Map map, Rot4 rot, CellPattern cellPattern);
}