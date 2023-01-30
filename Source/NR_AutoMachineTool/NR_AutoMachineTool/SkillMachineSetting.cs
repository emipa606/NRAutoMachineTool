using System;
using System.Collections.Generic;
using Verse;

namespace NR_AutoMachineTool;

public class SkillMachineSetting : BasicMachineSetting
{
    public int skillLevel;

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref skillLevel, "skillLevel");
    }

    protected override IEnumerable<Action<Listing>> ListDrawAction()
    {
        yield return delegate(Listing list) { DrawSkillLevel(list, ref skillLevel); };
        foreach (var item in base.ListDrawAction())
        {
            yield return item;
        }
    }
}