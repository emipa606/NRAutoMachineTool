using System.Collections.Generic;
using System.Linq;
using NR_AutoMachineTool.Utilities;
using RimWorld;
using Verse;

namespace NR_AutoMachineTool;

public class Building_Cleaner : Building_BaseRange<Filth>
{
    [Unsaved] private Option<Effecter> workingEffect = Ops.Nothing<Effecter>();

    public Building_Cleaner()
    {
        startCheckIntervalTicks = 15;
    }

    protected override float SpeedFactor => Setting.cleanerSetting.speedFactor;

    public override int MinPowerForSpeed => Setting.cleanerSetting.minSupplyPowerForSpeed;

    public override int MaxPowerForSpeed => Setting.cleanerSetting.maxSupplyPowerForSpeed;

    protected override bool WorkInterruption(Filth working)
    {
        return !working.Spawned;
    }

    protected override bool TryStartWorking(out Filth target, out float workAmount)
    {
        var targetCells = GetTargetCells();
        targetCells.SelectMany(c => c.GetThingList(Map).ToList()).SelectMany(t => Ops.Option(t as Pawn))
            .ForEach(delegate(Pawn p) { p.filth.TryDropFilth(); });
        target = (from t in targetCells.SelectMany(c => c.GetThingList(Map))
            where t.def.category == ThingCategory.Filth
            select t).SelectMany(t => Ops.Option(t as Filth)).FirstOption().GetOrDefault(null);
        if (target != null)
        {
            workAmount = target.def.filth.cleaningWorkToReduceThickness * target.thickness;
        }
        else
        {
            workAmount = 0f;
        }

        return target != null;
    }

    protected override bool FinishWorking(Filth working, out List<Thing> products)
    {
        products = [];
        working.Destroy();
        return true;
    }

    protected override void CleanupWorkingEffect()
    {
        base.CleanupWorkingEffect();
        workingEffect.ForEach(delegate(Effecter e) { e.Cleanup(); });
        workingEffect = Ops.Nothing<Effecter>();
        MapManager.RemoveEachTickAction(EffectTick);
    }

    protected override void CreateWorkingEffect()
    {
        base.CreateWorkingEffect();
        workingEffect = workingEffect.Fold(() => Ops.Option(EffecterDefOf.Clean.Spawn()))(Ops.Option);
        MapManager.EachTickAction(EffectTick);
    }

    private bool EffectTick()
    {
        workingEffect.ForEach(delegate(Effecter e) { e.EffectTick(new TargetInfo(Working), TargetInfo.Invalid); });
        return !workingEffect.HasValue;
    }
}