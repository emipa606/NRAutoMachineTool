namespace NR_AutoMachineTool;

public interface IRangePowerSupplyMachine
{
    int MinPowerForSpeed { get; }

    int MaxPowerForSpeed { get; }

    int MinPowerForRange { get; }

    int MaxPowerForRange { get; }

    float SupplyPowerForSpeed { get; set; }

    float SupplyPowerForRange { get; set; }

    bool Glowable { get; }

    bool Glow { get; set; }

    bool SpeedSetting { get; }
}