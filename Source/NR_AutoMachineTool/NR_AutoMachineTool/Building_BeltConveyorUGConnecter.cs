using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using NR_AutoMachineTool.Utilities;
using UnityEngine;
using Verse;

namespace NR_AutoMachineTool;

internal class Building_BeltConveyorUGConnecter : Building_BaseMachine<Thing>, IBeltConbeyorLinkable
{
    protected override float SpeedFactor => Setting.beltConveyorSetting.speedFactor;

    private ModExtension_AutoMachineTool Extension => def.GetModExtension<ModExtension_AutoMachineTool>();

    public override float SupplyPowerForSpeed
    {
        get => Building_BeltConveyor.supplyPower;
        set => Building_BeltConveyor.supplyPower = (int)value;
    }

    public override int MinPowerForSpeed => Setting.beltConveyorSetting.minSupplyPowerForSpeed;

    public override int MaxPowerForSpeed => Setting.beltConveyorSetting.maxSupplyPowerForSpeed;

    private bool ToUnderground => Extension.toUnderground;

    [field: Unsaved] public bool IsStuck { get; private set; }

    public IEnumerable<Rot4> OutputRots
    {
        get { yield return base.Rotation; }
    }

    public bool IsUnderground => false;

    public bool ReceiveThing(bool underground, Thing t)
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

        ForceStartWork(t, 1f);
        return true;
    }

    public void Link(IBeltConbeyorLinkable link)
    {
    }

    public void Unlink(IBeltConbeyorLinkable unlink)
    {
    }

    public bool ReceivableNow(bool underground, Thing thing)
    {
        if (!IsActive() || ToUnderground == underground)
        {
            return false;
        }

        return State switch
        {
            WorkingState.Ready => true,
            WorkingState.Working => false,
            WorkingState.Placing => Func(products[0]),
            _ => false
        };

        bool Func(Thing t)
        {
            return t.CanStackWith(thing) && t.stackCount < t.def.stackLimit;
        }
    }

    [SpecialName] Rot4 IBeltConbeyorLinkable.Rotation => base.Rotation;

    public override void SpawnSetup(Map map, bool respawningAfterLoad)
    {
        base.SpawnSetup(map, respawningAfterLoad);
        if (!respawningAfterLoad)
        {
            LinkTargetConveyor().ForEach(delegate(IBeltConbeyorLinkable x)
            {
                x.Link(this);
                Link(x);
            });
        }
    }

    public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
    {
        var list = LinkTargetConveyor();
        base.DeSpawn(mode);
        list.ForEach(delegate(IBeltConbeyorLinkable x) { x.Unlink(this); });
    }

    public override void DrawGUIOverlay()
    {
        base.DrawGUIOverlay();
        if (State == 0 || Find.CameraDriver.CurrentZoom != CameraZoomRange.Closest)
        {
            return;
        }

        var vector = CarryPosition();
        if (ToUnderground && !(WorkLeft > 0.7f))
        {
            return;
        }

        Vector2 screenPos = Find.Camera.WorldToScreenPoint(vector + new Vector3(0f, 0f, -0.4f)) / Prefs.UIScale;
        screenPos.y = UI.screenHeight - screenPos.y;
        GenMapUI.DrawThingLabel(screenPos, CarryingThing().stackCount.ToStringCached(),
            GenMapUI.DefaultThingLabelColor);
    }


    public override void DrawAt(Vector3 drawLoc, bool flip = false)
    {
        base.DrawAt(drawLoc, flip);
        if (State == 0)
        {
            return;
        }

        if (!ToUnderground || WorkLeft > 0.7f)
        {
            CarryingThing().DrawAt(CarryPosition());
        }
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
        var num = IsStuck ? Mathf.Clamp(Mathf.Abs(WorkLeft), 0f, 0.8f) : Mathf.Clamp01(WorkLeft);
        return (base.Rotation.FacingCell.ToVector3() * (1f - num)) + Position.ToVector3() +
               new Vector3(0.5f, 10f, 0.5f);
    }

    public override bool CanStackWith(Thing other)
    {
        if (base.CanStackWith(other))
        {
            return State == WorkingState.Ready;
        }

        return false;
    }

    protected override bool PlaceProduct(ref List<Thing> products)
    {
        var thing = products[0];
        var option = OutputConveyor();
        if (option.HasValue)
        {
            if (option.Value.ReceiveThing(ToUnderground, thing))
            {
                IsStuck = false;
                return true;
            }
        }
        else if (!ToUnderground && Ops.PlaceItem(thing, base.Rotation.FacingCell + Position, false, Map))
        {
            IsStuck = false;
            return true;
        }

        IsStuck = true;
        return false;
    }

    private List<IBeltConbeyorLinkable> LinkTargetConveyor()
    {
        return (from t in new List<Rot4>
            {
                base.Rotation,
                base.Rotation.Opposite
            }.Select(r => Position + r.FacingCell).SelectMany(t => t.GetThingList(Map))
            where t.def.category == ThingCategory.Building
            where Building_BeltConveyor.CanLink(this, t, def, t.def)
            select t).SelectMany(t => Ops.Option(t as IBeltConbeyorLinkable)).ToList();
    }

    private Option<IBeltConbeyorLinkable> OutputConveyor()
    {
        return (from x in LinkTargetConveyor()
            where x.Position == Position + base.Rotation.FacingCell
            select x).FirstOption();
    }

    protected override bool WorkInterruption(Thing working)
    {
        return false;
    }

    protected override bool TryStartWorking(out Thing target, out float workAmount)
    {
        target = null;
        workAmount = 1f;
        return false;
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

    public static bool IsConveyorUGConnecterDef(ThingDef def)
    {
        return typeof(Building_BeltConveyorUGConnecter).IsAssignableFrom(def.thingClass);
    }

    public static bool ToUndergroundDef(ThingDef def)
    {
        return Ops.Option(def.GetModExtension<ModExtension_AutoMachineTool>()).Fold(false)(x => x.toUnderground);
    }
}