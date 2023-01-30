using RimWorld;
using UnityEngine;
using Verse;

namespace NR_AutoMachineTool;

internal class ITab_PowerSupply : ITab
{
    private static readonly Vector2 WinSize = new Vector2(600f, 250f);

    private readonly string description;

    public ITab_PowerSupply()
    {
        size = WinSize;
        labelKey = "NR_AutoMachineTool.SupplyPowerTab";
        description = "NR_AutoMachineTool.SupplyPowerForSpeedText".Translate();
    }

    public IPowerSupplyMachine Machine => (IPowerSupplyMachine)SelThing;

    public override void FillTab()
    {
        var num = Machine.MinPowerForSpeed < 1000 ? 100 : Machine.MinPowerForSpeed < 10000 ? 500 : 1000;
        if (Machine.MinPowerForSpeed % num != 0 || Machine.MaxPowerForSpeed % num != 0)
        {
            num = 1;
        }

        var label = string.Concat("NR_AutoMachineTool.SupplyPowerValueLabel".Translate() + " (",
            Machine.MinPowerForSpeed.ToString(), " to ", Machine.MaxPowerForSpeed.ToString(), ") ");
        var listing_Standard = new Listing_Standard();
        var rect = new Rect(0f, 0f, WinSize.x, WinSize.y).ContractedBy(10f);
        listing_Standard.Begin(rect);
        listing_Standard.Gap();
        Widgets.Label(listing_Standard.GetRect(50f), description);
        listing_Standard.Gap();
        var num2 = (int)Widgets.HorizontalSlider_NewTemp(listing_Standard.GetRect(50f), Machine.SupplyPowerForSpeed,
            Machine.MinPowerForSpeed, Machine.MaxPowerForSpeed, true, label, Machine.MinPowerForSpeed.ToString(),
            Machine.MaxPowerForSpeed.ToString(), num);
        Machine.SupplyPowerForSpeed = num2;
        listing_Standard.Gap();
        var rect2 = listing_Standard.GetRect(30f);
        var buffer = Machine.SupplyPowerForSpeed.ToString();
        var val = (int)Machine.SupplyPowerForSpeed;
        Widgets.Label(rect2.LeftHalf(), label);
        Widgets.TextFieldNumeric(rect2.RightHalf(), ref val, ref buffer, Machine.MinPowerForSpeed,
            Machine.MaxPowerForSpeed);
        listing_Standard.Gap();
        listing_Standard.End();
        Machine.SupplyPowerForSpeed = val;
    }
}