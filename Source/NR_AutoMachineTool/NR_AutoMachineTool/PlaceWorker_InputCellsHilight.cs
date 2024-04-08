using System.Collections.Generic;
using System.Linq;
using NR_AutoMachineTool.Utilities;
using UnityEngine;
using Verse;

namespace NR_AutoMachineTool;

internal class PlaceWorker_InputCellsHilight : PlaceWorker
{
    public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot, Color ghostCol, Thing thing = null)
    {
        var map = Find.CurrentMap;
        var ext = def.GetModExtension<ModExtension_AutoMachineTool>();
        if (ext?.InputCellResolver == null)
        {
            Debug.LogWarning("inputCellResolver not found.");
            return;
        }

        ext.InputCellResolver.InputCell(center, map, rot).ForEach(delegate(IntVec3 c)
        {
            GenDraw.DrawFieldEdges(new List<IntVec3>().Append(c),
                ext.InputCellResolver.GetColor(c, map, rot, CellPattern.InputCell));
        });
        (from c in ext.InputCellResolver.InputZoneCells(center, map, rot)
            select new
            {
                Cell = c,
                Color = ext.InputCellResolver.GetColor(c, map, rot, CellPattern.InputZone)
            }
            into a
            group a by a.Color).ForEach(g => { GenDraw.DrawFieldEdges(g.Select(a => a.Cell).ToList(), g.Key); });
    }
}