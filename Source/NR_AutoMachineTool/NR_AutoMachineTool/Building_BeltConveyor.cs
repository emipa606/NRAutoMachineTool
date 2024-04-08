using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using NR_AutoMachineTool.Utilities;
using RimWorld;
using UnityEngine;
using Verse;

namespace NR_AutoMachineTool;

internal class Building_BeltConveyor : Building_BaseMachine<Thing>, IBeltConbeyorLinkable
{
    public static float supplyPower = 10f;
    private Rot4 dest;

    private Dictionary<Rot4, ThingFilter> filters = new Dictionary<Rot4, ThingFilter>();

    [Unsaved] private List<Rot4> outputRot = [];

    private Dictionary<Rot4, DirectionPriority> priorities = new[]
    {
        Rot4.North,
        Rot4.East,
        Rot4.South,
        Rot4.West
    }.ToDictionary(d => d, _ => DirectionPriority.Normal);

    [Unsaved] private int round;

    public Building_BeltConveyor()
    {
        setInitialMinPower = false;
    }

    private ModExtension_AutoMachineTool Extension => def.GetModExtension<ModExtension_AutoMachineTool>();

    protected override float SpeedFactor => Setting.beltConveyorSetting.speedFactor;

    public override int MinPowerForSpeed => Setting.beltConveyorSetting.minSupplyPowerForSpeed;

    public override int MaxPowerForSpeed => Setting.beltConveyorSetting.maxSupplyPowerForSpeed;

    public override float SupplyPowerForSpeed
    {
        get => supplyPower;
        set
        {
            supplyPower = value;
            SetPower();
        }
    }

    public Dictionary<Rot4, ThingFilter> Filters => filters;

    public Dictionary<Rot4, DirectionPriority> Priorities => priorities;

    public IEnumerable<Rot4> OutputRots => outputRot;

    [field: Unsaved] public bool IsStuck { get; private set; }

    public bool IsUnderground => Ops.Option(Extension).Fold(false)(x => x.underground);

    public bool ReceiveThing(bool underground, Thing t)
    {
        return ReceiveThing(underground, t, Destination(t, true));
    }

    public void Link(IBeltConbeyorLinkable link)
    {
        FilterSetting();
    }

    public void Unlink(IBeltConbeyorLinkable unlink)
    {
        FilterSetting();
        Ops.Option(Working).ForEach(delegate(Thing t) { dest = Destination(t, true); });
    }

    public bool ReceivableNow(bool underground, Thing thing)
    {
        if (!IsActive() || IsUnderground != underground)
        {
            return false;
        }

        return State switch
        {
            WorkingState.Ready => true,
            WorkingState.Working => Func(Working),
            WorkingState.Placing => Func(products[0]),
            _ => false
        };

        bool Func(Thing t)
        {
            return t.CanStackWith(thing) && t.stackCount < t.def.stackLimit;
        }
    }

    [SpecialName] Rot4 IBeltConbeyorLinkable.Rotation => Rotation;

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref supplyPower, "supplyPower", 10f);
        Scribe_Values.Look(ref dest, "dest");
        Scribe_Collections.Look(ref filters, "filters", LookMode.Value, LookMode.Deep);
        if (filters == null)
        {
            filters = new Dictionary<Rot4, ThingFilter>();
        }

        Scribe_Collections.Look(ref priorities, "priorities", LookMode.Value, LookMode.Value);
        if (priorities == null)
        {
            priorities = new[]
            {
                Rot4.North,
                Rot4.East,
                Rot4.South,
                Rot4.West
            }.ToDictionary(d => d, _ => DirectionPriority.Normal);
        }
    }

    public override void PostMapInit()
    {
        base.PostMapInit();
        FilterSetting();
    }

    public override void SpawnSetup(Map map, bool respawningAfterLoad)
    {
        base.SpawnSetup(map, respawningAfterLoad);
        if (respawningAfterLoad)
        {
            return;
        }

        var list = LinkTargetConveyor();
        if (list.Count == 0)
        {
            FilterSetting();
            return;
        }

        list.ForEach(delegate(IBeltConbeyorLinkable x)
        {
            x.Link(this);
            Link(x);
        });
    }

    public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
    {
        var list = LinkTargetConveyor();
        base.DeSpawn(mode);
        list.ForEach(delegate(IBeltConbeyorLinkable x) { x.Unlink(this); });
    }

    protected override void Reset()
    {
        if (State != 0)
        {
            FilterSetting();
        }

        base.Reset();
    }

    public override void DrawGUIOverlay()
    {
        base.DrawGUIOverlay();
        if (IsUnderground && !OverlayDrawHandler_UGConveyor.ShouldDraw || State == 0 ||
            Find.CameraDriver.CurrentZoom != CameraZoomRange.Closest)
        {
            return;
        }

        var vector = CarryPosition();
        Vector2 screenPos = Find.Camera.WorldToScreenPoint(vector + new Vector3(0f, 0f, -0.4f)) / Prefs.UIScale;
        screenPos.y = UI.screenHeight - screenPos.y;
        GenMapUI.DrawThingLabel(screenPos, CarryingThing().stackCount.ToStringCached(),
            GenMapUI.DefaultThingLabelColor);
    }

    public override void DrawAt(Vector3 drawLoc, bool flip = false)
    {
        if (IsUnderground && !OverlayDrawHandler_UGConveyor.ShouldDraw)
        {
            return;
        }

        base.DrawAt(drawLoc, flip);
        if (State == 0)
        {
            return;
        }

        CarryingThing().DrawAt(CarryPosition());
    }

    private Thing CarryingThing()
    {
        switch (State)
        {
            case WorkingState.Working:
                return Working;
            case WorkingState.Placing:
                return products[0];
            default:
                return null;
        }
    }

    private Vector3 CarryPosition()
    {
        var num = IsStuck ? Mathf.Clamp(Mathf.Abs(WorkLeft), 0f, 0.5f) : Mathf.Clamp01(WorkLeft);
        return (dest.FacingCell.ToVector3() * (1f - num)) + Position.ToVector3() + new Vector3(0.5f, 10f, 0.5f);
    }

    public override bool CanStackWith(Thing other)
    {
        if (base.CanStackWith(other))
        {
            return State == WorkingState.Ready;
        }

        return false;
    }

    private bool ReceiveThing(bool underground, Thing t, Rot4 rot)
    {
        if (!ReceivableNow(underground, t))
        {
            return false;
        }

        if (State != WorkingState.Ready)
        {
            return (State == WorkingState.Working ? Working : products[0]).TryAbsorbStack(t, true);
        }

        if (t.Spawned)
        {
            t.DeSpawn();
        }

        dest = rot;
        ForceStartWork(t, 1f);
        return true;
    }

    private Rot4 Destination(Thing t, bool doRotate)
    {
        var conveyors = OutputBeltConveyor();
        var list = (from f in filters
            where f.Value.Allows(t.def)
            select f.Key).ToList();
        var list2 = list.Where(r => (from b in conveyors.Where(l => l.Position == Position + r.FacingCell).FirstOption()
            select b.ReceivableNow(IsUnderground, t) || !b.IsStuck).GetOrDefault(true)).ToList();
        if (list2.Count == 0)
        {
            list2 = list.Count != 0
                ? list
                : OutputRots.Where(r =>
                    (from b in conveyors.Where(l => l.Position == Position + r.FacingCell).FirstOption()
                        select b.ReceivableNow(IsUnderground, t) || !b.IsStuck).GetOrDefault(true)).ToList();
        }

        var maxPri = list2.Select(r => priorities[r]).Max();
        var list3 = list2.Where(r => priorities[r] == maxPri).ToList();
        if (list3.Count <= round)
        {
            round = 0;
        }

        var index = round;
        if (doRotate)
        {
            round++;
        }

        return list3.ElementAt(index);
    }

    private bool SendableConveyor(Thing t, out Rot4 dir)
    {
        dir = default;
        var option = (from b in (from f in filters
                where f.Value.Allows(t.def)
                select f.Key).SelectMany(r => from l in OutputBeltConveyor()
                where l.Position == Position + r.FacingCell
                select l
                into b
                select new
                {
                    Dir = r,
                    Conveyor = b
                })
            where b.Conveyor.ReceivableNow(IsUnderground, t)
            select b).FirstOption();
        if (option.HasValue)
        {
            dir = option.Value.Dir;
        }

        return option.HasValue;
    }

    protected override bool PlaceProduct(ref List<Thing> products)
    {
        var thing = products[0];
        var option = (from o in LinkTargetConveyor()
            where o.Position == dest.FacingCell + Position
            select o).FirstOption();
        if (option.HasValue)
        {
            if (option.Value.ReceiveThing(IsUnderground, thing))
            {
                NotifyAroundSender();
                IsStuck = false;
                return true;
            }
        }
        else if (!IsUnderground && Ops.PlaceItem(thing, dest.FacingCell + Position, false, Map))
        {
            NotifyAroundSender();
            IsStuck = false;
            return true;
        }

        if (SendableConveyor(thing, out var dir))
        {
            Reset();
            ReceiveThing(IsUnderground, thing, dir);
            return false;
        }

        IsStuck = true;
        return false;
    }

    private void FilterSetting()
    {
        var output = OutputBeltConveyor();
        filters = (from x in Enumerable.Range(0, 4)
            select new Rot4(x)
            into x
            select new
            {
                Rot = x,
                Pos = Position + x.FacingCell
            }
            into x
            where output.Any(l => l.Position == x.Pos) || Rotation == x.Rot
            select x).ToDictionary(r => r.Rot, r => !filters.TryGetValue(r.Rot, out var filter) ? CreateNew() : filter);
        if (filters.Count <= 1)
        {
            filters.ForEach(delegate(KeyValuePair<Rot4, ThingFilter> x) { x.Value.SetAllowAll(null); });
        }

        outputRot = filters.Select(x => x.Key).ToList();
        return;

        ThingFilter CreateNew()
        {
            var thingFilter = new ThingFilter();
            thingFilter.SetAllowAll(null);
            return thingFilter;
        }
    }

    private List<IBeltConbeyorLinkable> LinkTargetConveyor()
    {
        return (from t in (from i in Enumerable.Range(0, 4)
                select Position + new Rot4(i).FacingCell).SelectMany(t => t.GetThingList(Map))
            where t.def.category == ThingCategory.Building
            where CanLink(this, t, def, t.def)
            select t).SelectMany(t => Ops.Option(t as IBeltConbeyorLinkable)).ToList();
    }

    private List<IBeltConbeyorLinkable> OutputBeltConveyor()
    {
        var links = LinkTargetConveyor();
        return links.Where(x =>
            x.Rotation.Opposite.FacingCell + x.Position == Position &&
            x.Position != Position + Rotation.Opposite.FacingCell ||
            x.Rotation.Opposite.FacingCell + x.Position == Position &&
            links.Any(l => l.Position + l.Rotation.FacingCell == Position)).ToList();
    }

    public bool Acceptable(Rot4 rot, bool underground)
    {
        if (rot != Rotation)
        {
            return IsUnderground == underground;
        }

        return false;
    }

    private void NotifyAroundSender()
    {
        (from t in new[]
            {
                Rotation.Opposite,
                Rotation.Opposite.RotateAsNew(RotationDirection.Clockwise),
                Rotation.Opposite.RotateAsNew(RotationDirection.Opposite)
            }.Select(r => Position + r.FacingCell).SelectMany(p => p.GetThingList(Map).ToList())
            where t.def.category == ThingCategory.Building
            select t).SelectMany(t => Ops.Option(t as IBeltConbeyorSender)).ForEach(delegate(IBeltConbeyorSender s)
        {
            s.NortifyReceivable();
        });
    }

    protected override bool WorkInterruption(Thing working)
    {
        return false;
    }

    protected override bool TryStartWorking(out Thing target, out float workAmount)
    {
        workAmount = 1f;
        if (IsUnderground)
        {
            target = null;
            return false;
        }

        target = (from t in Position.GetThingList(Map)
            where t.def.category == ThingCategory.Item
            where t.def != ThingDefOf.ActiveDropPod
            select t).FirstOption().GetOrDefault(null);
        if (target == null)
        {
            return target != null;
        }

        dest = Destination(target, true);
        if (target.Spawned)
        {
            target.DeSpawn();
        }

        return target != null;
    }

    protected override bool FinishWorking(Thing working, out List<Thing> products)
    {
        products = new List<Thing>().Append(working);
        return true;
    }

    protected override bool WorkingIsDespawned()
    {
        return true;
    }

    public static bool IsBeltConveyorDef(ThingDef def)
    {
        return typeof(Building_BeltConveyor).IsAssignableFrom(def.thingClass);
    }

    public static bool IsUndergroundDef(ThingDef def)
    {
        return Ops.Option(def.GetModExtension<ModExtension_AutoMachineTool>()).Fold(false)(x => x.underground);
    }

    public static bool CanLink(Thing @this, Thing other, ThingDef thisDef, ThingDef otherDef)
    {
        if (IsBeltConveyorDef(thisDef))
        {
            var isUndergroundDef = IsUndergroundDef(thisDef);
            if (IsBeltConveyorDef(otherDef))
            {
                if (isUndergroundDef != IsUndergroundDef(otherDef))
                {
                    return false;
                }

                if (!(@this.Position + @this.Rotation.FacingCell == other.Position) &&
                    !(@this.Position + @this.Rotation.Opposite.FacingCell == other.Position) &&
                    !(other.Position + other.Rotation.FacingCell == @this.Position))
                {
                    return other.Position + other.Rotation.Opposite.FacingCell == @this.Position;
                }

                return true;
            }

            if (!Building_BeltConveyorUGConnecter.IsConveyorUGConnecterDef(otherDef))
            {
                return false;
            }

            if (@this.Position + @this.Rotation.FacingCell == other.Position ||
                other.Position + other.Rotation.FacingCell == @this.Position &&
                isUndergroundDef == Building_BeltConveyorUGConnecter.ToUndergroundDef(otherDef))
            {
                return true;
            }

            if (other.Position + other.Rotation.Opposite.FacingCell == @this.Position)
            {
                return isUndergroundDef != Building_BeltConveyorUGConnecter.ToUndergroundDef(otherDef);
            }

            return false;
        }

        if (!Building_BeltConveyorUGConnecter.IsConveyorUGConnecterDef(thisDef))
        {
            return false;
        }

        var undergroundDef = Building_BeltConveyorUGConnecter.ToUndergroundDef(thisDef);
        if (IsBeltConveyorDef(otherDef))
        {
            if (@this.Position + @this.Rotation.FacingCell == other.Position &&
                undergroundDef == IsUndergroundDef(otherDef))
            {
                return true;
            }

            if (@this.Position + @this.Rotation.Opposite.FacingCell == other.Position)
            {
                return undergroundDef != IsUndergroundDef(otherDef);
            }

            return false;
        }

        if (!Building_BeltConveyorUGConnecter.IsConveyorUGConnecterDef(otherDef))
        {
            return false;
        }

        if (@this.Position + @this.Rotation.FacingCell == other.Position &&
            undergroundDef != Building_BeltConveyorUGConnecter.ToUndergroundDef(otherDef))
        {
            return true;
        }

        if (@this.Position + @this.Rotation.Opposite.FacingCell == other.Position)
        {
            return undergroundDef != Building_BeltConveyorUGConnecter.ToUndergroundDef(otherDef);
        }

        return false;
    }
}