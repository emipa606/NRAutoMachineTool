using System.Collections.Generic;
using System.Linq;
using NR_AutoMachineTool.Utilities;
using UnityEngine;
using Verse;

namespace NR_AutoMachineTool;

internal class PlaceWorker_OutputCellsHilight : PlaceWorker
{
    public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot, Color ghostCol, Thing thing = null)
    {
        var map = Find.CurrentMap;
        var ext = def.GetModExtension<ModExtension_AutoMachineTool>();
        if (ext == null || ext.OutputCellResolver == null)
        {
            Debug.LogWarning("outputCellResolver not found.");
            return;
        }

        ext.OutputCellResolver.OutputCell(center, map, rot).ForEach(delegate(IntVec3 c)
        {
            GenDraw.DrawFieldEdges(new List<IntVec3>().Append(c),
                ext.OutputCellResolver.GetColor(c, map, rot, CellPattern.OutputCell));
        });
        (from c in ext.OutputCellResolver.OutputZoneCells(center, map, rot)
            select new
            {
                Cell = c,
                Color = ext.OutputCellResolver.GetColor(c, map, rot, CellPattern.OutputZone)
            }
            into a
            group a by a.Color).ForEach(g => { GenDraw.DrawFieldEdges(g.Select(a => a.Cell).ToList(), g.Key); });
    }
}