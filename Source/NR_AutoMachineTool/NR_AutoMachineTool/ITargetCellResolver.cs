using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace NR_AutoMachineTool;

public interface ITargetCellResolver
{
    ModExtension_AutoMachineTool Parent { get; set; }

    int MaxPowerForRange { get; }

    int MinPowerForRange { get; }

    bool NeedClearingCache { get; }

    IEnumerable<IntVec3> GetRangeCells(IntVec3 pos, Map map, Rot4 rot, int range);

    Color GetColor(IntVec3 cell, Map map, Rot4 rot, CellPattern cellPattern);

    int GetRange(float power);
}