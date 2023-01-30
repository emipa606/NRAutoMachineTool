using RimWorld;
using UnityEngine;
using Verse;

namespace NR_AutoMachineTool;

internal class ITab_RangePowerSupply : ITab
{
    private static readonly Vector2 WinSize = new Vector2(600f, 380f);

    private readonly string descriptionForRange;

    private readonly string descriptionForSpeed;

    public ITab_RangePowerSupply()
    {
        size = WinSize;
        labelKey = "NR_AutoMachineTool.SupplyPowerTab";
        descriptionForSpeed = "NR_AutoMachineTool.SupplyPowerForSpeedText".Translate();
        descriptionForRange = "NR_AutoMachineTool.SupplyPowerForRangeText".Translate();
    }

    private IRangePowerSupplyMachine Machine => (IRangePowerSupplyMachine)SelThing;

    public override void FillTab()
    {
        var minPowerForSpeed = Machine.MinPowerForSpeed;
        var maxPowerForSpeed = Machine.MaxPowerForSpeed;
        var minPowerForRange = Machine.MinPowerForRange;
        var maxPowerForRange = Machine.MaxPowerForRange;
        var label = string.Concat("NR_AutoMachineTool.SupplyPowerForSpeedValueLabel".Translate() + " (",
            minPowerForSpeed.ToString(), " to ", maxPowerForSpeed.ToString(), ") ");
        var label2 = string.Concat("NR_AutoMachineTool.SupplyPowerForRangeValueLabel".Translate() + " (",
            minPowerForRange.ToString(), " to ", maxPowerForRange.ToString(), ") ");
        var listing_Standard = new Listing_Standard();
        var rect = new Rect(0f, 0f, WinSize.x, WinSize.y).ContractedBy(10f);
        listing_Standard.Begin(rect);
        listing_Standard.Gap();
        if (Machine.SpeedSetting)
        {
            Widgets.Label(listing_Standard.GetRect(50f), descriptionForSpeed);
            listing_Standard.Gap();
            var num = (int)Widgets.HorizontalSlider_NewTemp(listing_Standard.GetRect(50f), Machine.SupplyPowerForSpeed,
                minPowerForSpeed, maxPowerForSpeed, true, label, minPowerForSpeed.ToString(),
                maxPowerForSpeed.ToString(), 100f);
            Machine.SupplyPowerForSpeed = num;
            listing_Standard.Gap();
            var rect2 = listing_Standard.GetRect(30f);
            var buffer = Machine.SupplyPowerForSpeed.ToString();
            var val = (int)Machine.SupplyPowerForSpeed;
            Widgets.Label(rect2.LeftHalf(), label);
            Widgets.TextFieldNumeric(rect2.RightHalf(), ref val, ref buffer, Machine.SupplyPowerForSpeed,
                Machine.SupplyPowerForSpeed);
            listing_Standard.Gap();
            Machine.SupplyPowerForSpeed = val;
        }

        Widgets.Label(listing_Standard.GetRect(50f), descriptionForRange);
        listing_Standard.Gap();
        var num2 = (int)Widgets.HorizontalSlider_NewTemp(listing_Standard.GetRect(50f), Machine.SupplyPowerForRange,
            minPowerForRange, maxPowerForRange, true, label2, minPowerForRange.ToString(), maxPowerForRange.ToString(),
            500f);
        Machine.SupplyPowerForRange = num2;
        listing_Standard.Gap();
        if (Machine.Glowable)
        {
            var rect3 = listing_Standard.GetRect(30f);
            var checkOn = Machine.Glow;
            Widgets.CheckboxLabeled(rect3, "NR_AutoMachineTool.SupplyPowerSunLampText".Translate(), ref checkOn);
            Machine.Glow = checkOn;
        }

        listing_Standard.Gap();
        listing_Standard.End();
    }
}