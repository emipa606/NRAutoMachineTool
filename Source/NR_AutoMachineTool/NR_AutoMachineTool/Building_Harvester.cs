using System.Collections.Generic;
using System.Linq;
using NR_AutoMachineTool.Utilities;
using RimWorld;
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
        if (!working.Spawned)
        {
            return true;
        }

        if (!working.Blighted)
        {
            return !working.HarvestableNow;
        }

        return false;
    }

    protected override bool TryStartWorking(out Plant target, out float workAmount)
    {
        target = (from p in (from c in GetTargetCells()
                where c.GetPlantable(Map).HasValue
                select c).SelectMany(c => c.GetThingList(Map)).SelectMany(t => Ops.Option(t as Plant))
            where Harvestable(p)
            select p).FirstOption().GetOrDefault(null);
        workAmount = target?.def.plant.harvestWork ?? 0f;
        return target != null;
    }

    private bool Harvestable(Plant p)
    {
        if (p.Blighted)
        {
            return true;
        }

        if (p.HarvestableNow && p.LifeStage == PlantLifeStage.Mature && !InWorking(p))
        {
            return !IsLimit(p.def.plant.harvestedThingDef);
        }

        return false;
    }

    protected override bool FinishWorking(Plant working, out List<Thing> products)
    {
        products = new List<Thing>();
        working.def.plant.soundHarvestFinish.PlayOneShot(this);
        if (!working.Blighted)
        {
            products = CreateThings(working.def.plant.harvestedThingDef, working.YieldNow());
            working.PlantCollected(null, PlantDestructionMode.Cut);
        }
        else
        {
            working.Destroy();
        }

        return true;
    }
}