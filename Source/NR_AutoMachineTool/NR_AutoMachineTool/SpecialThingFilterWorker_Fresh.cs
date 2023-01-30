using RimWorld;
using Verse;

namespace NR_AutoMachineTool;

public class SpecialThingFilterWorker_Fresh : SpecialThingFilterWorker
{
    public override bool Matches(Thing t)
    {
        if (!(t is ThingWithComps thingWithComps))
        {
            return false;
        }

        var comp = thingWithComps.GetComp<CompRottable>();
        if (comp != null && !((CompProperties_Rottable)comp.props).rotDestroys)
        {
            return comp.Stage == RotStage.Fresh;
        }

        return false;
    }

    public override bool CanEverMatch(ThingDef def)
    {
        var compProperties = def.GetCompProperties<CompProperties_Rottable>();
        if (compProperties != null)
        {
            return !compProperties.rotDestroys;
        }

        return false;
    }
}