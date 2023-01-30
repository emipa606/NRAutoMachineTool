namespace NR_AutoMachineTool;

internal interface IPowerSupplyMachine
{
    int MinPowerForSpeed { get; }

    int MaxPowerForSpeed { get; }

    float SupplyPowerForSpeed { get; set; }
}