using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace NR_AutoMachineTool;

public abstract class BaseTargetCellResolver : ITargetCellResolver
{
    protected ModSetting_AutoMachineTool Setting => LoadedModManager.GetMod<Mod_AutoMachineTool>().Setting;

    public abstract int MinPowerForRange { get; }

    public abstract int MaxPowerForRange { get; }

    public abstract bool NeedClearingCache { get; }

    public ModExtension_AutoMachineTool Parent { get; set; }

    public virtual int GetRange(float power)
    {
        return Mathf.RoundToInt(power / 500f) + 3;
    }

    public virtual Color GetColor(IntVec3 cell, Map map, Rot4 rot, CellPattern cellPattern)
    {
        return cellPattern.ToColor();
    }

    public abstract IEnumerable<IntVec3> GetRangeCells(IntVec3 pos, Map map, Rot4 rot, int range);
}