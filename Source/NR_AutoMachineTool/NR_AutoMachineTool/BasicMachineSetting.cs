using System;
using System.Collections.Generic;
using Verse;

namespace NR_AutoMachineTool;

public class BasicMachineSetting : BaseMachineSetting
{
    public int maxSupplyPowerForSpeed;
    public int minSupplyPowerForSpeed;

    public float speedFactor;

    public override void ExposeData()
    {
        Scribe_Values.Look(ref minSupplyPowerForSpeed, "minSupplyPowerForSpeed", 100);
        Scribe_Values.Look(ref maxSupplyPowerForSpeed, "maxSupplyPowerForSpeed", 10000);
        Scribe_Values.Look(ref speedFactor, "speedFactor");
    }

    protected override IEnumerable<Action<Listing>> ListDrawAction()
    {
        yield return delegate(Listing list)
        {
            DrawPower(list, "NR_AutoMachineTool.SettingMinSupplyPower", "NR_AutoMachineTool.Speed",
                ref minSupplyPowerForSpeed, 0f, 100000f);
        };
        yield return delegate(Listing list)
        {
            DrawPower(list, "NR_AutoMachineTool.SettingMaxSupplyPower", "NR_AutoMachineTool.Speed",
                ref maxSupplyPowerForSpeed, 0f, 10000000f);
        };
        yield return delegate(Listing list) { DrawSpeedFactor(list, ref speedFactor); };
    }

    protected override void FinishDrawModSetting()
    {
        if (minSupplyPowerForSpeed > maxSupplyPowerForSpeed)
        {
            minSupplyPowerForSpeed = maxSupplyPowerForSpeed;
        }
    }
}