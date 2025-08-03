using System.Collections.Generic;
using System.Linq;
using NR_AutoMachineTool.Utilities;
using RimWorld;
using Verse;

namespace NR_AutoMachineTool;

public class Building_Stunner : Building_BaseRange<Pawn>
{
    [Unsaved] private Effecter lightning;

    [Unsaved] private int lightningCount;

    [Unsaved] private Pawn target;

    public Building_Stunner()
    {
        readyOnStart = true;
        targetEnumrationCount = 0;
    }

    protected override float SpeedFactor => Setting.stunnerSetting.speedFactor;

    public override int MinPowerForSpeed => Setting.stunnerSetting.minSupplyPowerForSpeed;

    public override int MaxPowerForSpeed => Setting.stunnerSetting.maxSupplyPowerForSpeed;

    protected override bool WorkInterruption(Pawn working)
    {
        if (working.Spawned)
        {
            return !GetAllTargetCells().Contains(working.Position);
        }

        return true;
    }

    protected override TargetInfo ProgressBarTarget()
    {
        return new TargetInfo(this);
    }

    protected override bool TryStartWorking(out Pawn target, out float workAmount)
    {
        var allCells = GetAllTargetCells();
        var source = (from p in Map.mapPawns.AllPawns
            where allCells.Contains(p.Position)
            where p.Faction != Faction.OfPlayer
            where !p.Dead && !p.Downed
            where !InWorking(p)
            select p).ToList();
        var first = from p in source
            where p.Faction.HostileTo(Faction.OfPlayer)
            where !p.IsPrisoner || p.IsPrisoner && PrisonBreakUtility.IsPrisonBreaking(p)
            where p.IsPrisoner || p.CurJobDef != JobDefOf.Goto || !p.CurJob.exitMapOnArrival
            select p;
        var second = source.Where(p =>
            p.MentalStateDef == MentalStateDefOf.Manhunter || p.MentalStateDef == MentalStateDefOf.ManhunterPermanent);
        target = first.Concat(second).FirstOption().GetOrDefault(null);
        workAmount = 3000f;
        return target != null;
    }

    protected override bool FinishWorking(Pawn working, out List<Thing> products)
    {
        var named = DefDatabase<HediffDef>.GetNamed("NR_AutoMachineTool_Hediff_Unconsciousness");
        if (working.Faction == Faction.OfMechanoids)
        {
            working.Kill(null);
        }
        else
        {
            var hediff = HediffMaker.MakeHediff(named, working);
            working.health.AddHediff(hediff);
        }

        products = [];
        lightningCount = 15;
        target = working;
        lightning = DefDatabase<EffecterDef>.GetNamed("NR_AutoMachineTool_Effect_Lightning").Spawn();
        lightning.EffectTick(new TargetInfo(this), new TargetInfo(target));
        MapManager.EachTickAction(Lightning);
        return true;
    }

    private bool Lightning()
    {
        lightningCount--;
        var num = lightningCount <= 0;
        if (num && lightning != null)
        {
            lightning.Cleanup();
        }

        return num;
    }
}