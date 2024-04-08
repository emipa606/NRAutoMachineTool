using System.Linq;
using NR_AutoMachineTool.Utilities;
using UnityEngine;
using Verse;

namespace NR_AutoMachineTool;

internal class PlaceWorker_TargetCellsHilight : PlaceWorker
{
    public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot, Color ghostCol, Thing thing = null)
    {
        var map = Find.CurrentMap;
        var ext = def.GetModExtension<ModExtension_AutoMachineTool>();
        if (ext?.TargetCellResolver == null)
        {
            Debug.LogWarning("targetCellResolver not found.");
            return;
        }

        var option = (from t in center.GetThingList(map)
            where t.def == def
            select t).SelectMany(t => Ops.Option(t as IRange)).FirstOption();
        if (option.HasValue)
        {
            (from c in option.Value.GetAllTargetCells()
                select new
                {
                    Cell = c,
                    Color = ext.TargetCellResolver.GetColor(c, map, rot, CellPattern.Instance)
                }
                into a
                group a by a.Color).ForEach(g => { GenDraw.DrawFieldEdges(g.Select(a => a.Cell).ToList(), g.Key); });
        }
        else
        {
            var rangeCells = ext.TargetCellResolver.GetRangeCells(center, map, rot, ext.TargetCellResolver.MinRange());
            (from a in Enumerable.Concat(
                    second: from c in ext.TargetCellResolver.GetRangeCells(center, map, rot,
                        ext.TargetCellResolver.MaxRange())
                    select new
                    {
                        Cell = c,
                        Color = ext.TargetCellResolver.GetColor(c, map, rot, CellPattern.BlurprintMax)
                    }, first: rangeCells.Select(c => new
                    {
                        Cell = c,
                        Color = ext.TargetCellResolver.GetColor(c, map, rot, CellPattern.BlurprintMin)
                    }))
                group a by a.Color).ForEach(g => { GenDraw.DrawFieldEdges(g.Select(a => a.Cell).ToList(), g.Key); });
        }

        (from r in map.listerThings.ThingsOfDef(def).SelectMany(t => Ops.Option(t as IRange))
            where r.Position != center
            select r).ForEach(delegate(IRange r)
        {
            (from c in r.GetAllTargetCells()
                select new
                {
                    Cell = c,
                    Color = ext.TargetCellResolver.GetColor(c, map, rot, CellPattern.OtherInstance)
                }
                into a
                group a by a.Color).ForEach(g => { GenDraw.DrawFieldEdges(g.Select(a => a.Cell).ToList(), g.Key); });
        });
    }
}