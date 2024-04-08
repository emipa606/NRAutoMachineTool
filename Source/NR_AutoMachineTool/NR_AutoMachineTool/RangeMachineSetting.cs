using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace NR_AutoMachineTool;

public class RangeMachineSetting : BasicMachineSetting
{
    public int maxSupplyPowerForRange;
    public int minSupplyPowerForRange;

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref minSupplyPowerForRange, "minSupplyPowerForRange");
        Scribe_Values.Look(ref maxSupplyPowerForRange, "maxSupplyPowerForRange", 5000);
    }

    protected override IEnumerable<Action<Listing>> ListDrawAction()
    {
        var actions = base.ListDrawAction().ToList();
        yield return actions[0];
        yield return actions[1];
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
        yield return actions[2];
    }

    protected override void FinishDrawModSetting()
    {
        base.FinishDrawModSetting();
        if (minSupplyPowerForRange > maxSupplyPowerForRange)
        {
            minSupplyPowerForRange = maxSupplyPowerForRange;
        }
    }
}