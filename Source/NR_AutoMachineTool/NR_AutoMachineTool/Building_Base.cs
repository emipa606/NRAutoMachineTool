using System;
using System.Collections.Generic;
using System.Linq;
using NR_AutoMachineTool.Utilities;
using UnityEngine;
using Verse;

namespace NR_AutoMachineTool;

public abstract class Building_Base<T> : Building, IProductOutput where T : Thing
{
    private static readonly HashSet<T> workingSet = [];

    protected bool forcePlace = true;

    [Unsaved] protected bool placeFirstAbsorb;

    protected List<Thing> products = [];

    [Unsaved] private Effecter progressBar;

    [Unsaved] protected bool readyOnStart;

    [Unsaved] protected bool showProgressBar = true;

    [Unsaved] protected int startCheckIntervalTicks = 30;

    private WorkingState state;

    private float totalWorkAmount;

    private T working;

    private int workStartTick;

    protected ModSetting_AutoMachineTool Setting => ModSetting;

    protected static ModSetting_AutoMachineTool ModSetting => LoadedModManager.GetMod<Mod_AutoMachineTool>().Setting;

    protected WorkingState State
    {
        get => state;
        private set
        {
            if (state == value)
            {
                return;
            }

            OnChangeState(state, value);
            state = value;
        }
    }

    protected T Working => working;

    protected MapTickManager MapManager { get; private set; }

    protected float CurrentWorkAmount => (Find.TickManager.TicksAbs - workStartTick) * WorkAmountPerTick;

    protected float WorkLeft => totalWorkAmount - CurrentWorkAmount;

    protected abstract float WorkAmountPerTick { get; }

    public virtual IntVec3 OutputCell()
    {
        return Position + Rotation.Opposite.FacingCell;
    }

    protected virtual void OnChangeState(WorkingState before, WorkingState after)
    {
    }

    protected virtual void ClearActions()
    {
        MapManager.RemoveAfterAction(Ready);
        MapManager.RemoveAfterAction(Placing);
        MapManager.RemoveAfterAction(CheckWork);
        MapManager.RemoveAfterAction(StartWork);
        MapManager.RemoveAfterAction(FinishWork);
    }

    protected virtual bool WorkingIsDespawned()
    {
        return false;
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref state, "workingState");
        Scribe_Values.Look(ref totalWorkAmount, "totalWorkAmount");
        Scribe_Values.Look(ref workStartTick, "workStartTick");
        Scribe_Collections.Look(ref products, "products", LookMode.Deep);
        if (WorkingIsDespawned())
        {
            Scribe_Deep.Look(ref working, "working");
        }
        else
        {
            Scribe_References.Look(ref working, "working");
        }
    }

    public override void PostMapInit()
    {
        base.PostMapInit();
        if (products == null)
        {
            products = [];
        }

        if (working == null && State == WorkingState.Working)
        {
            ForceReady();
        }

        if (products.Count == 0 && State == WorkingState.Placing)
        {
            ForceReady();
        }
    }

    protected static bool InWorking(T thing)
    {
        return workingSet.Contains(thing);
    }

    public override void SpawnSetup(Map map, bool respawningAfterLoad)
    {
        base.SpawnSetup(map, respawningAfterLoad);
        MapManager = map.GetComponent<MapTickManager>();
        if (readyOnStart)
        {
            State = WorkingState.Ready;
            Reset();
            MapManager.AfterAction(Rand.Range(0, startCheckIntervalTicks), Ready);
        }
        else if (State == WorkingState.Ready)
        {
            MapManager.AfterAction(Rand.Range(0, startCheckIntervalTicks), Ready);
        }
        else if (State == WorkingState.Working)
        {
            MapManager.NextAction(StartWork);
        }
        else if (State == WorkingState.Placing)
        {
            MapManager.NextAction(Placing);
        }
    }

    public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
    {
        Reset();
        ClearActions();
        base.DeSpawn(mode);
    }

    protected virtual bool IsActive()
    {
        return !Destroyed && Spawned;
    }

    protected virtual void Reset()
    {
        if (State != 0)
        {
            products.ForEach(delegate(Thing t)
            {
                if (!t.Spawned)
                {
                    GenPlace.TryPlaceThing(t, Position, Map, ThingPlaceMode.Near);
                }
            });
        }

        CleanupWorkingEffect();
        State = WorkingState.Ready;
        totalWorkAmount = 0f;
        workStartTick = 0;
        Ops.Option(working).ForEach(delegate(T h) { workingSet.Remove(h); });
        working = null;
        products.Clear();
    }

    protected void ForceReady()
    {
        Reset();
        ClearActions();
        MapManager.NextAction(Ready);
    }

    protected virtual void CleanupWorkingEffect()
    {
        Ops.Option(progressBar).ForEach(delegate(Effecter e) { e.Cleanup(); });
        progressBar = null;
    }

    protected virtual void CreateWorkingEffect()
    {
        CleanupWorkingEffect();
        if (!working.Spawned || !showProgressBar)
        {
            return;
        }

        Ops.Option(ProgressBarTarget()).ForEach(delegate
        {
            progressBar = DefDatabase<EffecterDef>.GetNamed("NR_AutoMachineTool_Effect_ProgressBar").Spawn();
            progressBar.EffectTick(ProgressBarTarget(), TargetInfo.Invalid);
            ((MoteProgressBar2)((SubEffecter_ProgressBar)progressBar.children[0]).mote).progressGetter =
                () => CurrentWorkAmount / totalWorkAmount;
        });
    }

    protected virtual TargetInfo ProgressBarTarget()
    {
        return working;
    }

    protected virtual void Ready()
    {
        if (State != WorkingState.Ready || !Spawned)
        {
            return;
        }

        if (!IsActive())
        {
            Reset();
            MapManager.AfterAction(30, Ready);
        }
        else if (TryStartWorking(out working, out totalWorkAmount))
        {
            State = WorkingState.Working;
            workStartTick = Find.TickManager.TicksAbs;
            MapManager.NextAction(StartWork);
        }
        else
        {
            MapManager.AfterAction(startCheckIntervalTicks, Ready);
        }
    }

    private int CalcRemainTick()
    {
        return float.IsInfinity(totalWorkAmount)
            ? int.MaxValue
            : Mathf.Max(1, Mathf.CeilToInt((totalWorkAmount - CurrentWorkAmount) / WorkAmountPerTick));
    }

    protected virtual void StartWork()
    {
        if (State != WorkingState.Working || !Spawned)
        {
            return;
        }

        if (!IsActive())
        {
            ForceReady();
            return;
        }

        CreateWorkingEffect();
        MapManager.AfterAction(30, CheckWork);
        if (!float.IsInfinity(totalWorkAmount))
        {
            MapManager.AfterAction(CalcRemainTick(), FinishWork);
        }
    }

    protected void ForceStartWork(T working, float workAmount)
    {
        Reset();
        ClearActions();
        State = WorkingState.Working;
        this.working = working;
        totalWorkAmount = workAmount;
        workStartTick = Find.TickManager.TicksAbs;
        MapManager.NextAction(StartWork);
    }

    protected virtual void CheckWork()
    {
        if (State != WorkingState.Working || !Spawned)
        {
            return;
        }

        if (!IsActive())
        {
            ForceReady();
        }
        else if (WorkInterruption(working))
        {
            ForceReady();
        }
        else if (CurrentWorkAmount >= totalWorkAmount)
        {
            MapManager.NextAction(FinishWork);
        }
        else
        {
            MapManager.AfterAction(30, CheckWork);
        }
    }

    protected virtual void FinishWork()
    {
        if (State != WorkingState.Working || !Spawned)
        {
            return;
        }

        MapManager.RemoveAfterAction(CheckWork);
        MapManager.RemoveAfterAction(FinishWork);
        if (!IsActive())
        {
            ForceReady();
        }
        else if (WorkInterruption(working))
        {
            ForceReady();
        }
        else if (FinishWorking(working, out products))
        {
            State = WorkingState.Placing;
            CleanupWorkingEffect();
            working = null;
            if (products == null || products.Count == 0)
            {
                Reset();
                MapManager.NextAction(Ready);
            }
            else
            {
                MapManager.NextAction(Placing);
            }
        }
        else
        {
            Reset();
            MapManager.NextAction(Ready);
        }
    }

    protected virtual void Placing()
    {
        if (State != WorkingState.Placing || !Spawned)
        {
            return;
        }

        if (!IsActive())
        {
            ForceReady();
        }
        else if (PlaceProduct(ref products))
        {
            State = WorkingState.Ready;
            Reset();
            MapManager.NextAction(Ready);
        }
        else
        {
            MapManager.AfterAction(30, Placing);
        }
    }

    protected abstract bool WorkInterruption(T working);

    protected abstract bool TryStartWorking(out T target, out float workAmount);

    protected abstract bool FinishWorking(T working, out List<Thing> products);

    protected virtual bool PlaceProduct(ref List<Thing> products)
    {
        products = products.Aggregate([], delegate(List<Thing> total, Thing target)
        {
            var option = (from b in (from t in OutputCell().GetThingList(Map)
                    where t.def.category == ThingCategory.Building
                    select t).SelectMany(t => Ops.Option(t as IBeltConbeyorLinkable))
                where !b.IsUnderground
                select b).FirstOption();
            if (option.HasValue)
            {
                if (option.Value.ReceiveThing(false, target))
                {
                    return total;
                }
            }
            else
            {
                if (target.Spawned)
                {
                    target.DeSpawn();
                }

                if (Ops.PlaceItem(target, OutputCell(), false, Map, placeFirstAbsorb))
                {
                    return total;
                }

                if (!forcePlace)
                {
                    return total.Append(target);
                }

                GenPlace.TryPlaceThing(target, OutputCell(), Map, ThingPlaceMode.Near);
                return total;
            }

            return total.Append(target);
        });
        return this.products.Count == 0;
    }

    public override string GetInspectString()
    {
        var inspectString = base.GetInspectString();
        inspectString += "\n";
        switch (State)
        {
            case WorkingState.Working:
                if (float.IsInfinity(totalWorkAmount))
                {
                    return inspectString + "NR_AutoMachineTool.StatWorkingNotParam".Translate();
                }

                return inspectString + "NR_AutoMachineTool.StatWorking".Translate(
                    Mathf.RoundToInt(Math.Min(CurrentWorkAmount, totalWorkAmount)), Mathf.RoundToInt(totalWorkAmount),
                    Mathf.RoundToInt(Mathf.Clamp01(CurrentWorkAmount / totalWorkAmount)) * 100);
            case WorkingState.Ready:
                return inspectString + "NR_AutoMachineTool.StatReady".Translate();
            case WorkingState.Placing:
                return inspectString + "NR_AutoMachineTool.StatPlacing".Translate(products.Count);
            default:
                return inspectString + State;
        }
    }

    public void NortifyReceivable()
    {
        if (State != WorkingState.Placing || !Spawned || MapManager.IsExecutingThisTick(Placing))
        {
            return;
        }

        MapManager.RemoveAfterAction(Placing);
        MapManager.NextAction(Placing);
    }

    protected List<Thing> CreateThings(ThingDef def, int count)
    {
        var quot = count / def.stackLimit;
        var remain = count % def.stackLimit;
        return Enumerable.Range(0, quot + 1).Select((_, i) => i != quot ? def.stackLimit : remain).Select(
                delegate(int c)
                {
                    var thing = ThingMaker.MakeThing(def);
                    thing.stackCount = c;
                    return thing;
                })
            .ToList();
    }
}