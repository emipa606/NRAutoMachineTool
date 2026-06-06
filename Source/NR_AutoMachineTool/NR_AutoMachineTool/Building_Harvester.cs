using System.Collections.Generic;
using System.Linq;
using NR_AutoMachineTool.Utilities;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace NR_AutoMachineTool;

public class Building_Harvester : Building_BaseRange<Plant>
{
    protected override float SpeedFactor => Setting.HarvesterTier(Extension.tier).speedFactor;

    public override int MinPowerForSpeed => Setting.HarvesterTier(Extension.tier).minSupplyPowerForSpeed;

    public override int MaxPowerForSpeed => Setting.HarvesterTier(Extension.tier).maxSupplyPowerForSpeed;

    protected override bool WorkInterruption(Plant working)
    {
        return !IsCutTarget(working);
    }

    protected override bool TryStartWorking(out Plant target, out float workAmount)
    {
        target = (from p in (from c in GetTargetCells()
                where c.GetPlantable(Map).HasValue
                select c).SelectMany(c => c.GetThingList(Map)).SelectMany(t => Ops.Option(t as Plant))
            where IsCutTarget(p)
            select p).FirstOption().GetOrDefault(null);
        workAmount = target == null ? 0f : Mathf.Max(1f, target.def.plant.harvestWork);
        return target != null;
    }

    private bool IsCutTarget(Plant p)
    {
        if (!p.Spawned || InWorking(p))
        {
            return false;
        }

        if (p.Blighted)
        {
            return true;
        }

        var plantable = p.Position.GetPlantable(Map);
        if (!plantable.HasValue)
        {
            return false;
        }

        if (plantable.Value.GetPlantDefToGrow() != p.def)
        {
            return true;
        }

        if (p.LifeStage == PlantLifeStage.Mature && p.def.plant.harvestedThingDef == null)
        {
            return true;
        }

        if (p.HarvestableNow && p.LifeStage == PlantLifeStage.Mature)
        {
            return !IsLimit(p.def.plant.harvestedThingDef);
        }

        return false;
    }

    protected override bool FinishWorking(Plant working, out List<Thing> products)
    {
        products = [];
        working.def.plant.soundHarvestFinish.PlayOneShot(this);
        if (working.Blighted)
        {
            working.Destroy();
            return true;
        }

        if (working.HarvestableNow && working.def.plant.harvestedThingDef != null)
        {
            products = CreateThings(working.def.plant.harvestedThingDef, working.YieldNow());
        }

        var by = Map.mapPawns.FreeColonistsSpawned.FirstOrDefault() ??
                 Map.mapPawns.SpawnedPawnsInFaction(Faction.OfPlayer).FirstOrDefault();

        if (by != null)
        {
            working.PlantCollected(by, PlantDestructionMode.Cut);
            return true;
        }

        if (working.def.plant.HarvestDestroys)
        {
            working.TrySpawnStump(PlantDestructionMode.Cut);
            working.Destroy(DestroyMode.KillFinalizeLeavingsOnly);
        }
        else
        {
            working.Growth = working.def.plant.harvestAfterGrowth;
            working.Map.mapDrawer.MapMeshDirty(working.Position, MapMeshFlagDefOf.Things);
        }

        return true;
    }
}