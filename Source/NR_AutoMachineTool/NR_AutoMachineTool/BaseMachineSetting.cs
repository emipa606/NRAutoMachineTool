using System;
using System.Collections.Generic;
using System.Linq;
using NR_AutoMachineTool.Utilities;
using Verse;

namespace NR_AutoMachineTool;

public abstract class BaseMachineSetting : IMachineSetting
{
    public void DrawModSetting(Listing list)
    {
        ListDrawAction().ForEach(delegate(Action<Listing> a)
        {
            a(list);
            list.Gap();
        });
        FinishDrawModSetting();
    }

    public abstract void ExposeData();

    public float GetHeight()
    {
        return ListDrawAction().Count() * 42f;
    }

    protected abstract IEnumerable<Action<Listing>> ListDrawAction();

    protected virtual void FinishDrawModSetting()
    {
    }

    protected static void DrawPower(Listing list, string label, string labelParm, ref int power, float min, float max)
    {
        string buffer = null;
        var rect = list.GetRect(30f);
        Widgets.Label(rect.LeftHalf(), label.Translate(labelParm.Translate()));
        Widgets.TextFieldNumeric(rect.RightHalf(), ref power, ref buffer, min, max);
    }

    protected static void DrawSpeedFactor(Listing list, ref float factor)
    {
        var rect = list.GetRect(30f);
        Widgets.Label(rect.LeftHalf(), "NR_AutoMachineTool.SettingSpeedFactor".Translate(factor.ToString("F1")));
        factor = Widgets.HorizontalSlider(rect.RightHalf(), factor, 0.1f, 10f, true,
            "NR_AutoMachineTool.SettingSpeedFactor".Translate(factor.ToString("F1")), 0.1f.ToString("F1"),
            10f.ToString("F1"), 0.1f);
    }

    protected static void DrawSkillLevel(Listing list, ref int skillLevel)
    {
        var rect = list.GetRect(30f);
        Widgets.Label(rect.LeftHalf(), "NR_AutoMachineTool.SettingSkillLevel".Translate(skillLevel));
        skillLevel = (int)Widgets.HorizontalSlider(rect.RightHalf(), skillLevel, 1f, 20f, true,
            "NR_AutoMachineTool.SettingSkillLevel".Translate(skillLevel), 1.ToString(), 20.ToString(), 1f);
    }
}