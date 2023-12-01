using System.Collections.Generic;
using System.Linq;
using NR_AutoMachineTool.Utilities;
using RimWorld;
using Verse;

namespace NR_AutoMachineTool;

public class Building_Planter : Building_BaseRange<Thing>
{
    protected override int? SkillLevel => Setting.PlanterTier(Extension.tier).skillLevel;

    protected override float SpeedFactor => Setting.PlanterTier(Extension.tier).speedFactor;

    public override int MinPowerForSpeed => Setting.PlanterTier(Extension.tier).minSupplyPowerForSpeed;

    public override int MaxPowerForSpeed => Setting.PlanterTier(Extension.tier).maxSupplyPowerForSpeed;

    public override bool Glowable => true;

    protected override void Reset()
    {
        if (Working is { Spawned: true })
        {
            Working.Destroy();
        }

        base.Reset();
    }

    protected override bool WorkInterruption(Thing working)
    {
        return !working.Spawned;
    }

    protected override bool TryStartWorking(out Thing target, out float workAmount)
    {
        target = (from c in GetTargetCells()
            select new
            {
                Cell = c,
                Plantable = c.GetPlantable(Map)
            }
            into a
            where a.Plantable.HasValue
            select new
            {
                a.Cell,
                Plantable = a.Plantable.Value
            }
            into c
            where CanSow(c.Cell, c.Plantable)
            where !IsLimit(c.Plantable.GetPlantDefToGrow().plant.harvestedThingDef)
            select c).FirstOption().SelectMany(c =>
        {
            var thing = ThingMaker.MakeThing(c.Plantable.GetPlantDefToGrow());
            Ops.Option(thing as Plant).ForEach(delegate(Plant x) { x.sown = false; });
            return !GenPlace.TryPlaceThing(thing, c.Cell, Map, ThingPlaceMode.Direct)
                ? Ops.Nothing<Thing>()
                : Ops.Option(thing);
        }).GetOrDefault(null);
        workAmount = target?.def.plant.sowWork ?? 0f;
        return target != null;
    }

    protected override bool FinishWorking(Thing working, out List<Thing> products)
    {
        Ops.Option(working as Plant).ForEach(delegate(Plant x) { x.sown = true; });
        products = [];
        return true;
    }

    private bool CanSow(IntVec3 cell, IPlantToGrowSettable grower)
    {
        if (!grower.GetPlantDefToGrow().CanEverPlantAt(cell, Map) || !PlantUtility.GrowthSeasonNow(cell, Map) ||
            !PlantUtility.SnowAllowsPlanting(cell, Map) ||
            !Ops.Option(grower as Zone_Growing).Fold(true)(z => z.allowSow) ||
            !(grower.GetPlantDefToGrow().plant.sowMinSkill <= SkillLevel) ||
            PlantUtility.AdjacentSowBlocker(grower.GetPlantDefToGrow(), cell, Map) != null)
        {
            return false;
        }

        if (!grower.GetPlantDefToGrow().plant.interferesWithRoof)
        {
            return true;
        }

        if (grower.GetPlantDefToGrow().plant.interferesWithRoof)
        {
            return !cell.Roofed(Map);
        }

        return false;
    }
}