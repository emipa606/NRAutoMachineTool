using System.Linq;
using NR_AutoMachineTool.Utilities;
using RimWorld;
using Verse;

namespace NR_AutoMachineTool;

public class PlaceWorker_AutoMachineTool : PlaceWorker
{
    public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map,
        Thing thingToIgnore = null, Thing thing = null)
    {
        var result = base.AllowsPlacing(checkingDef, loc, rot, map, thingToIgnore, thing);
        if (!result.Accepted)
        {
            return result;
        }

        return !(from b in (from t in (loc + rot.FacingCell).GetThingList(map)
                where t.def.category == ThingCategory.Building
                select t).SelectMany(t => Ops.Option(t as Building_WorkTable))
            where b.InteractionCell == loc
            select b).Any()
            ? new AcceptanceReport("NR_AutoMachineTool.PlaceNotAllowed".Translate())
            : result;
    }
}