using System.Collections.Generic;
using System.Linq;
using NR_AutoMachineTool.Utilities;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace NR_AutoMachineTool;

public class Building_Slaughterhouse : Building_BaseRange<Pawn>, ISlaughterhouse
{
    private Dictionary<ThingDef, SlaughterSettings> slaughterSettings = new Dictionary<ThingDef, SlaughterSettings>();

    protected override float SpeedFactor => Setting.slaughterSetting.speedFactor;

    public override int MinPowerForSpeed => Setting.slaughterSetting.minSupplyPowerForSpeed;

    public override int MaxPowerForSpeed => Setting.slaughterSetting.maxSupplyPowerForSpeed;

    public Dictionary<ThingDef, SlaughterSettings> Settings => slaughterSettings;

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Collections.Look(ref slaughterSettings, "slaughterSettings", LookMode.Def, LookMode.Deep);
    }

    protected override void Reset()
    {
        if (Working != null && Working.jobs.curJob.def == JobDefOf.Wait_MaintainPosture)
        {
            Working.jobs.EndCurrentJob(JobCondition.InterruptForced);
        }

        base.Reset();
    }

    private HashSet<Pawn> ShouldSlaughterPawns()
    {
        var mapPawns = Map.mapPawns.SpawnedPawnsInFaction(Faction.OfPlayer);
        return Ops.ToHashSet(slaughterSettings.Values.Where(s => s.doSlaughter).SelectMany(delegate(SlaughterSettings s)
        {
            var pawns = mapPawns.Where(p => p.def == s.def);

            return (from a in new[]
                {
                    new
                    {
                        Gender = Gender.Male,
                        Adult = true
                    },
                    new
                    {
                        Gender = Gender.Female,
                        Adult = true
                    },
                    new
                    {
                        Gender = Gender.Male,
                        Adult = false
                    },
                    new
                    {
                        Gender = Gender.Female,
                        Adult = false
                    }
                }
                select new
                {
                    Group = a,
                    Pawns = pawns.Where(p => p.gender == a.Gender && p.IsAdult() == a.Adult)
                }
                into g
                select new
                {
                    g.Group,
                    g.Pawns,
                    SlaughterCount = g.Pawns.Count() - s.KeepCount(g.Group.Gender, g.Group.Adult)
                }
                into g
                where g.SlaughterCount > 0
                select g).SelectMany(g => OrderBy(g.Pawns.Where(Where), g.Group.Adult).Take(g.SlaughterCount));

            bool Where(Pawn p)
            {
                var stillValid = true;
                if (!s.hasBonds)
                {
                    stillValid = p.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Bond) == null;
                }

                if (stillValid && !s.pregnancy)
                {
                    stillValid = p.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.Pregnant, true) == null;
                }

                if (stillValid && !s.trained)
                {
                    stillValid = !p.training.HasLearned(TrainableDefOf.Obedience);
                }

                return stillValid;
            }

            IOrderedEnumerable<Pawn> OrderBy(IEnumerable<Pawn> e, bool adult)
            {
                return adult
                    ? e.OrderByDescending(p => p.ageTracker.AgeChronologicalTicks)
                    : e.OrderBy(p => p.ageTracker.AgeChronologicalTicks);
            }
        }));
    }

    protected override bool WorkInterruption(Pawn working)
    {
        if (!working.Dead)
        {
            return !working.Spawned;
        }

        return true;
    }

    protected override bool TryStartWorking(out Pawn target, out float workAmount)
    {
        workAmount = 400f;
        target = null;
        var enumerable = from p in (from t in GetTargetCells().SelectMany(c => c.GetThingList(Map))
                where t.def.category == ThingCategory.Pawn
                select t).SelectMany(t => Ops.Option(t as Pawn))
            where !InWorking(p)
            where slaughterSettings.ContainsKey(p.def)
            select p;
        if (!enumerable.FirstOption().HasValue)
        {
            return false;
        }

        var targets = ShouldSlaughterPawns();
        target = enumerable.Where(p => targets.Contains(p)).FirstOption().GetOrDefault(null);
        if (target != null)
        {
            PawnUtility.ForceWait(target, 15000, null, true);
        }

        return target != null;
    }

    protected override bool FinishWorking(Pawn working, out List<Thing> products)
    {
        if (working.jobs.curJob.def == JobDefOf.Wait_MaintainPosture)
        {
            working.jobs.EndCurrentJob(JobCondition.InterruptForced);
        }

        var num = Mathf.Max(GenMath.RoundRandom(working.BodySize * 8f), 1);
        for (var i = 0; i < num; i++)
        {
            working.health.DropBloodFilth();
        }

        working.Kill(null);
        products = new List<Thing>().Append(working.Corpse);
        working.Corpse.DeSpawn();
        working.Corpse.SetForbidden(false);
        return true;
    }
}