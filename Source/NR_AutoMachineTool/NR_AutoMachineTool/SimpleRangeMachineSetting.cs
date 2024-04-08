using System;
using System.Collections.Generic;
using Verse;

namespace NR_AutoMachineTool;

public class SimpleRangeMachineSetting : BaseMachineSetting
{
    public int maxSupplyPowerForRange;
    public int minSupplyPowerForRange;

    public override void ExposeData()
    {
        Scribe_Values.Look(ref minSupplyPowerForRange, "minSupplyPowerForRange");
        Scribe_Values.Look(ref maxSupplyPowerForRange, "maxSupplyPowerForRange", 5000);
    }

    protected override IEnumerable<Action<Listing>> ListDrawAction()
    {
        yield return delegate(Listing list)
        {
            DrawPower(list, "NR_AutoMachineTool.SettingMinSupplyPower", "NR_AutoMachineTool.Range",
                ref minSupplyPowerForRange, 0f, 1000f);
        };
        yield return delegate(Listing list)
        {
            DrawPower(list, "NR_AutoMachineTool.SettingMaxSupplyPower", "NR_AutoMachineTool.Range",
                ref maxSupplyPowerForRange, 0f, 20000f);
        };
    }
}