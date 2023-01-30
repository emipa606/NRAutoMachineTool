using System.Collections.Generic;
using System.Linq;
using NR_AutoMachineTool.Utilities;
using RimWorld;
using Verse;
using Verse.AI;

namespace NR_AutoMachineTool;

public class Building_AnimalResourceGatherer : Building_BaseRange<Pawn>
{
    private CompHasGatherableBodyResource comp;

    private string compName;

    protected override float SpeedFactor => Setting.gathererSetting.speedFactor;

    public override int MinPowerForSpeed => Setting.gathererSetting.minSupplyPowerForSpeed;

    public override int MaxPowerForSpeed => Setting.gathererSetting.maxSupplyPowerForSpeed;

    protected override void Reset()
    {
        if (Working != null && Working.jobs.curJob.def == JobDefOf.Wait_MaintainPosture)
        {
            Working.jobs.EndCurrentJob(JobCondition.InterruptForced);
        }

        base.Reset();
    }

    public override void ExposeData()
    {
        base.ExposeData();
        if (comp != null)
        {
            compName = comp.GetType().FullName;
        }

        Scribe_Values.Look(ref compName, "compName");
        if (compName != null && Working != null)
        {
            comp = (from c in Working.GetComps<CompHasGatherableBodyResource>()
                where c.GetType().FullName == compName
                select c).FirstOption().GetOrDefault(null);
        }
    }

    protected override bool WorkInterruption(Pawn working)
    {
        return comp.Fullness < 0.5f;
    }

    protected override bool TryStartWorking(out Pawn target, out float workAmount)
    {
        var orDefault = (from a in (from p in GetTargetCells().SelectMany(c => c.GetThingList(Map))
                    .SelectMany(t => Ops.Option(t as Pawn))
                where p.Faction == Faction.OfPlayer
                where p.TryGetComp<CompHasGatherableBodyResource>() != null
                select p).SelectMany(a => from c in a.GetComps<CompHasGatherableBodyResource>()
                select new
                {
                    Animal = a,
                    Comp = c
                })
            where a.Comp.Fullness >= 0.5f
            where !IsLimit(a.Comp.ResourceDef)
            select a).FirstOption().GetOrDefault(null);
        target = null;
        workAmount = 0f;
        if (orDefault == null)
        {
            return false;
        }

        target = orDefault.Animal;
        comp = orDefault.Comp;
        switch (comp)
        {
            case CompShearable:
                workAmount = 1700f;
                break;
            case CompMilkable:
                workAmount = 400f;
                break;
        }

        workAmount = 1000f;
        PawnUtility.ForceWait(target, 15000, null, true);

        return true;
    }

    protected override bool FinishWorking(Pawn working, out List<Thing> products)
    {
        var thingDef = comp.ResourceDef;
        var count = GenMath.RoundRandom(comp.Fullness);
        products = CreateThings(thingDef, count);
        if (Working.jobs.curJob.def == JobDefOf.Wait_MaintainPosture)
        {
            Working.jobs.EndCurrentJob(JobCondition.InterruptForced);
        }

        comp.fullness = 0f;
        comp = null;
        return true;
    }
}