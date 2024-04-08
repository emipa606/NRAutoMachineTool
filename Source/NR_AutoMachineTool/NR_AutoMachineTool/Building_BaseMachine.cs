using System;
using RimWorld;
using Verse;

namespace NR_AutoMachineTool;

public abstract class Building_BaseMachine<T> : Building_Base<T>, IPowerSupplyMachine, IBeltConbeyorSender
    where T : Thing
{
    protected CompPowerTrader powerComp;

    [Unsaved] protected bool setInitialMinPower = true;

    private float supplyPowerForSpeed;

    protected abstract float SpeedFactor { get; }

    protected virtual int? SkillLevel => null;

    protected override float WorkAmountPerTick => 0.01f * SpeedFactor * SupplyPowerForSpeed * Factor2();

    public abstract int MinPowerForSpeed { get; }

    public abstract int MaxPowerForSpeed { get; }

    public virtual float SupplyPowerForSpeed
    {
        get => supplyPowerForSpeed;
        set
        {
            if (supplyPowerForSpeed == value)
            {
                return;
            }

            supplyPowerForSpeed = value;
            SetPower();
        }
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref supplyPowerForSpeed, "supplyPowerForSpeed", MinPowerForSpeed);
        ReloadSettings(null, null);
    }

    protected virtual void ReloadSettings(object sender, EventArgs e)
    {
        if (SupplyPowerForSpeed < MinPowerForSpeed)
        {
            SupplyPowerForSpeed = MinPowerForSpeed;
        }

        if (SupplyPowerForSpeed > MaxPowerForSpeed)
        {
            SupplyPowerForSpeed = MaxPowerForSpeed;
        }
    }

    public override void SpawnSetup(Map map, bool respawningAfterLoad)
    {
        base.SpawnSetup(map, respawningAfterLoad);
        powerComp = this.TryGetComp<CompPowerTrader>();
        if (!respawningAfterLoad && setInitialMinPower)
        {
            SupplyPowerForSpeed = MinPowerForSpeed;
        }

        LoadedModManager.GetMod<Mod_AutoMachineTool>().Setting.DataExposed += ReloadSettings;
        MapManager.NextAction(SetPower);
    }

    protected override bool IsActive()
    {
        return powerComp is { PowerOn: true } && base.IsActive();
    }

    public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
    {
        LoadedModManager.GetMod<Mod_AutoMachineTool>().Setting.DataExposed -= ReloadSettings;
        base.DeSpawn(mode);
    }

    protected virtual void SetPower()
    {
        powerComp.Props.basePowerConsumption = SupplyPowerForSpeed;
        powerComp.powerOutputInt = -SupplyPowerForSpeed;
    }

    protected virtual float Factor2()
    {
        return 0.1f;
    }
}