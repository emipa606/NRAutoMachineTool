using System.Collections.Generic;
using System.Linq;
using NR_AutoMachineTool.Utilities;
using RimWorld;
using UnityEngine;
using Verse;

namespace NR_AutoMachineTool;

public class Building_Repairer : Building_BaseRange<Building_Repairer>
{
    [Unsaved] private Pawn pawn;

    [Unsaved] private Effecter progressBar;

    [Unsaved] private float repairAmount;

    [Unsaved] private Thing working;

    public Building_Repairer()
    {
        forcePlace = false;
        readyOnStart = true;
        showProgressBar = false;
        targetEnumrationCount = 0;
    }

    protected override float SpeedFactor => Setting.repairerSetting.speedFactor;

    public override int MinPowerForSpeed => Setting.repairerSetting.minSupplyPowerForSpeed;

    public override int MaxPowerForSpeed => Setting.repairerSetting.maxSupplyPowerForSpeed;

    protected override bool WorkInterruption(Building_Repairer working)
    {
        return false;
    }

    protected override bool TryStartWorking(out Building_Repairer target, out float workAmount)
    {
        pawn = null;
        working = null;
        repairAmount = 0f;
        target = this;
        var cells = GetAllTargetCells();
        working ??= (from t in Map.listerThings.ThingsInGroup(ThingRequestGroup.Shell)
            where cells.Contains(t.Position)
            select t).SelectMany(t => Ops.Option(t as Fire)).FirstOption().GetOrDefault(null);

        if (working == null)
        {
            working = (from t in Map.listerBuildingsRepairable.RepairableBuildings(Faction.OfPlayer)
                where cells.Contains(t.Position)
                where t.def.category == ThingCategory.Building
                where t.HitPoints < t.MaxHitPoints
                select t).FirstOption().GetOrDefault(null);
        }

        if (working == null)
        {
            pawn = (from t in Map.mapPawns.SpawnedPawnsInFaction(Faction.OfPlayer)
                where cells.Contains(t.Position)
                select t
                into p
                where p.equipment is { AllEquipmentListForReading: not null }
                where p.equipment.AllEquipmentListForReading.Cast<Thing>().ToList()
                    .Append(p.apparel.WornApparel.Cast<Thing>().ToList())
                    .Any(t => t.HitPoints < t.MaxHitPoints)
                select p).FirstOption().GetOrDefault(null);
            if (pawn != null)
            {
                working = (from t in pawn.equipment.AllEquipmentListForReading.Cast<Thing>().ToList()
                        .Append(pawn.apparel.WornApparel.Cast<Thing>().ToList())
                    where t.HitPoints < t.MaxHitPoints
                    select t).First();
            }
        }

        workAmount = working == null ? 0f : float.PositiveInfinity;
        if (working == null)
        {
            return working != null;
        }

        progressBar = DefDatabase<EffecterDef>.GetNamed("NR_AutoMachineTool_Effect_ProgressBar").Spawn();
        progressBar.EffectTick(new TargetInfo(pawn ?? working), TargetInfo.Invalid);
        if (working is Fire)
        {
            ((MoteProgressBar2)((SubEffecter_ProgressBar)progressBar.children[0]).mote).progressGetter =
                () => (1.75f - ((Fire)working).fireSize) / 1.75f;
        }
        else
        {
            ((MoteProgressBar2)((SubEffecter_ProgressBar)progressBar.children[0]).mote).progressGetter =
                () => working.HitPoints / (float)working.MaxHitPoints;
        }

        MapManager.EachTickAction(Repair);

        return working != null;
    }

    protected override void Reset()
    {
        base.Reset();
        progressBar?.Cleanup();
    }

    private bool Repair()
    {
        var num = RepairInt();
        if (!num)
        {
            return false;
        }

        ForceReady();
        if (progressBar == null)
        {
            return true;
        }

        progressBar.Cleanup();
        progressBar = null;

        return true;
    }

    private bool RepairInt()
    {
        if (!IsActive())
        {
            return true;
        }

        if (pawn != null && (!pawn.Spawned ||
                             !GenRadial.RadialCellsAround(Position, GetRange(), true).Contains(pawn.Position)))
        {
            return true;
        }

        if (pawn == null && (!working.Spawned ||
                             !GenRadial.RadialCellsAround(Position, GetRange(), true).Contains(working.Position)))
        {
            return true;
        }

        if (working is Fire fire)
        {
            fire.fireSize -= WorkAmountPerTick * 0.05f;
            if (!(fire.fireSize <= 0f))
            {
                return false;
            }

            fire.fireSize = 0f;
            if (fire.Spawned)
            {
                fire.Destroy();
            }

            return true;
        }

        repairAmount += WorkAmountPerTick;
        if (repairAmount > 1f)
        {
            var num = Mathf.RoundToInt(repairAmount);
            working.HitPoints += num;
            repairAmount -= num;
        }

        if (working.HitPoints < working.MaxHitPoints)
        {
            return false;
        }

        working.HitPoints = working.MaxHitPoints;
        if (working is Building b)
        {
            Map.listerBuildingsRepairable.Notify_BuildingRepaired(b);
        }

        return true;
    }

    protected override float Factor2()
    {
        return base.Factor2() * 0.05f;
    }

    protected override void ClearActions()
    {
        base.ClearActions();
        MapManager.RemoveEachTickAction(Repair);
    }

    protected override bool FinishWorking(Building_Repairer working, out List<Thing> products)
    {
        products = [];
        return true;
    }
}