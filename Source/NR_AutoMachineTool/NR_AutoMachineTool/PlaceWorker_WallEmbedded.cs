using System.Linq;
using Verse;

namespace NR_AutoMachineTool;

public class PlaceWorker_WallEmbedded : PlaceWorker
{
    public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map,
        Thing thingToIgnore = null, Thing thing = null)
    {
        return (from t in loc.GetThingList(map)
            where t.def.category == ThingCategory.Building
            where t.def.building != null
            where !t.def.building.isNaturalRock
            where t.def.passability == Traversability.Impassable
            select t).Any(t => (t.def.graphicData.linkFlags & LinkFlags.Wall) == LinkFlags.Wall)
            ? AcceptanceReport.WasAccepted
            : new AcceptanceReport("NR_AutoMachineTool_Conveyor.MustInWall".Translate());
    }
}