using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using NR_AutoMachineTool.Utilities;
using RimWorld;
using Verse;

namespace NR_AutoMachineTool;

public abstract class Building_BaseRange<T> : Building_BaseLimitation<T>, IRange, IRangePowerSupplyMachine
    where T : Thing
{
    private const int CACHE_CLEAR_INTERVAL_TICKS = 180;

    [Unsaved] private HashSet<IntVec3> allTargetCellsCache;

    private bool glow;

    [Unsaved] private bool nextTargetCells;

    [Unsaved] private List<List<IntVec3>> splittedTargetCells;

    [Unsaved] private int splittedTargetCellsIndex;

    private float supplyPowerForRange;

    [Unsaved] protected int targetEnumrationCount = 100;

    protected ModExtension_AutoMachineTool Extension => def.GetModExtension<ModExtension_AutoMachineTool>();

    private bool SplitTargetCells
    {
        get
        {
            if (targetEnumrationCount > 0)
            {
                return GetAllTargetCells().Count() > targetEnumrationCount;
            }

            return false;
        }
    }

    public IEnumerable<IntVec3> GetAllTargetCells()
    {
        CacheTargetCells();
        return allTargetCellsCache;
    }

    public int GetRange()
    {
        return Extension.TargetCellResolver.GetRange(SupplyPowerForRange);
    }

    [SpecialName] Rot4 IRange.Rotation => base.Rotation;

    public int MinPowerForRange => Extension.TargetCellResolver.MinPowerForRange;

    public int MaxPowerForRange => Extension.TargetCellResolver.MaxPowerForRange;

    public virtual bool Glowable => false;

    public virtual bool Glow
    {
        get => glow;
        set
        {
            if (glow == value)
            {
                return;
            }

            glow = value;
            ChangeGlow();
        }
    }

    public virtual bool SpeedSetting => true;

    public float SupplyPowerForRange
    {
        get => supplyPowerForRange;
        set
        {
            if (supplyPowerForRange != value)
            {
                supplyPowerForRange = value;
                ChangeGlow();
                allTargetCellsCache = null;
            }

            SetPower();
        }
    }

    private void CacheTargetCells()
    {
        if (allTargetCellsCache != null)
        {
            return;
        }

        allTargetCellsCache =
            Ops.ToHashSet(Extension.TargetCellResolver.GetRangeCells(Position, Map, base.Rotation, GetRange()));
        if (targetEnumrationCount > 0)
        {
            splittedTargetCells = allTargetCellsCache.ToList().Grouped(targetEnumrationCount);
        }
    }

    private List<IntVec3> GetCurrentSplittedTargetCells()
    {
        CacheTargetCells();
        if (splittedTargetCellsIndex >= splittedTargetCells.Count)
        {
            splittedTargetCellsIndex = 0;
        }

        return splittedTargetCells[splittedTargetCellsIndex];
    }

    private void NextSplittedTargetCells()
    {
        splittedTargetCellsIndex++;
        if (splittedTargetCellsIndex >= splittedTargetCells.Count)
        {
            splittedTargetCellsIndex = 0;
        }
    }

    private void ClearAllTargetCellCache()
    {
        if (IsActive())
        {
            allTargetCellsCache = null;
        }

        if (Spawned && Extension.TargetCellResolver.NeedClearingCache)
        {
            MapManager.AfterAction(180, ClearAllTargetCellCache);
        }
    }

    protected override void ClearActions()
    {
        base.ClearActions();
        MapManager.RemoveAfterAction(ClearAllTargetCellCache);
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref supplyPowerForRange, "supplyPowerForRange", MinPowerForRange);
        Scribe_Values.Look(ref glow, "glow");
    }

    protected override void ReloadSettings(object sender, EventArgs e)
    {
        if (SupplyPowerForRange < MinPowerForRange)
        {
            SupplyPowerForRange = MinPowerForRange;
        }

        if (SupplyPowerForRange > MaxPowerForRange)
        {
            SupplyPowerForRange = MaxPowerForRange;
        }
    }

    protected override void SetPower()
    {
        var powerLevel = SupplyPowerForRange + SupplyPowerForSpeed + (Glowable && Glow ? 2000 : 0);
        if (this.TryGetComp<CompPowerTrader>().Props.basePowerConsumption == powerLevel)
        {
            return;
        }

        powerComp.Props.basePowerConsumption = powerLevel;
        powerComp.powerOutputInt = -powerLevel;
    }

    private void ChangeGlow()
    {
        Ops.Option(this.TryGetComp<CompGlower>()).ForEach(delegate(CompGlower glower)
        {
            var switchIsOn = this.TryGetComp<CompFlickable>().SwitchIsOn;
            glower.Props.glowRadius = Glow ? (GetRange() + 2f) * 2f : 0f;
            glower.Props.overlightRadius = Glow ? GetRange() + 2.1f : 0f;
            this.TryGetComp<CompFlickable>().SwitchIsOn = !switchIsOn;
            glower.UpdateLit(Map);
            this.TryGetComp<CompFlickable>().SwitchIsOn = switchIsOn;
            glower.UpdateLit(Map);
        });
    }

    public override void SpawnSetup(Map map, bool respawningAfterLoad)
    {
        base.SpawnSetup(map, respawningAfterLoad);
        if (!respawningAfterLoad)
        {
            SupplyPowerForRange = MinPowerForRange;
        }

        Ops.Option(this.TryGetComp<CompGlower>()).ForEach(delegate(CompGlower g)
        {
            g.props = new CompProperties_Glower
            {
                compClass = g.Props.compClass,
                glowColor = g.Props.glowColor,
                glowRadius = g.Props.glowRadius,
                overlightRadius = g.Props.overlightRadius
            };
        });
        allTargetCellsCache = null;
        ChangeGlow();
        if (Extension.TargetCellResolver.NeedClearingCache)
        {
            MapManager.AfterAction(180, ClearAllTargetCellCache);
        }
    }

    protected virtual IEnumerable<IntVec3> GetTargetCells()
    {
        if (!SplitTargetCells)
        {
            return GetAllTargetCells();
        }

        nextTargetCells = true;
        return GetCurrentSplittedTargetCells();
    }

    protected override void Ready()
    {
        base.Ready();
        if (State != WorkingState.Ready || !SplitTargetCells || !nextTargetCells)
        {
            return;
        }

        NextSplittedTargetCells();
        nextTargetCells = false;
    }
}