using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace NR_AutoMachineTool;

internal class ITab_Slaughterhouse : ITab
{
    private static readonly Vector2 WinSize = new Vector2(800f, 600f);

    private static readonly float[] ColumnWidth = [0.2f, 0.05f, 0.05f, 0.05f, 0.05f, 0.15f, 0.15f, 0.15f, 0.15f];

    private static readonly TipSignal slaughterTip =
        new TipSignal("NR_AutoMachineTool.SlaughterhouseSetting.DoSlaughterTip".Translate());

    private static readonly TipSignal hasBondsTip =
        new TipSignal("NR_AutoMachineTool.SlaughterhouseSetting.BondsTip".Translate());

    private static readonly TipSignal pregnancyTip =
        new TipSignal("NR_AutoMachineTool.SlaughterhouseSetting.PregnancyTip".Translate());

    private static readonly TipSignal trainedTip =
        new TipSignal("NR_AutoMachineTool.SlaughterhouseSetting.TrainedTip".Translate());

    private static readonly TipSignal keepMaleChildCountTip =
        new TipSignal("NR_AutoMachineTool.SlaughterhouseSetting.KeepCountTip".Translate(
            "NR_AutoMachineTool.Male".Translate(), "NR_AutoMachineTool.Young".Translate()));

    private static readonly TipSignal keepFemaleChildCountTip =
        new TipSignal("NR_AutoMachineTool.SlaughterhouseSetting.KeepCountTip".Translate(
            "NR_AutoMachineTool.Female".Translate(), "NR_AutoMachineTool.Young".Translate()));

    private static readonly TipSignal keepMaleAdultCountTip =
        new TipSignal("NR_AutoMachineTool.SlaughterhouseSetting.KeepCountTip".Translate(
            "NR_AutoMachineTool.Male".Translate(), "NR_AutoMachineTool.Adult".Translate()));

    private static readonly TipSignal keepFemaleAdultCountTip =
        new TipSignal("NR_AutoMachineTool.SlaughterhouseSetting.KeepCountTip".Translate(
            "NR_AutoMachineTool.Female".Translate(), "NR_AutoMachineTool.Adult".Translate()));

    private List<ThingDef> defs;

    private string description;

    private Vector2 scrollPosition;

    public ITab_Slaughterhouse()
    {
        size = WinSize;
        labelKey = "NR_AutoMachineTool.SlaughterhouseSetting.TabName";
    }

    public ISlaughterhouse Machine => (ISlaughterhouse)SelThing;

    public override void OnOpen()
    {
        base.OnOpen();
        defs = (from p in Find.CurrentMap.mapPawns.SpawnedPawnsInFaction(Faction.OfPlayer)
            where p.RaceProps.Animal && p.RaceProps.IsFlesh && p.SpawnedOrAnyParentSpawned
            select p.def).Distinct().ToList();
    }

    private Func<float, Rect> CutLeftFunc(Rect rect)
    {
        var curX = 0f;
        return delegate(float pct)
        {
            var result = new Rect(curX, rect.y, rect.width * pct, rect.height);
            curX += rect.width * pct;
            return result;
        };
    }

    public override void FillTab()
    {
        description = "NR_AutoMachineTool.SlaughterhouseSetting.Description".Translate();
        var rect = new Rect(0f, 0f, WinSize.x, WinSize.y).ContractedBy(10f);
        var listing_Standard = new Listing_Standard();
        listing_Standard.Begin(rect);
        var rect2 = listing_Standard.GetRect(40f);
        listing_Standard.Gap();
        Widgets.Label(rect2, description);
        var rect3 = listing_Standard.GetRect(24f);
        rect3.width -= 30f;
        var func = CutLeftFunc(rect3);
        var colIndex = 0;
        var col = func(ColumnWidth[colIndex++]);
        Widgets.Label(col, "NR_AutoMachineTool.RaceName".Translate());
        col = func(ColumnWidth[colIndex++]);
        GUI.DrawTexture(col.LeftPartPixels(24f), RS.SlaughterIcon);
        TooltipHandler.TipRegion(col, slaughterTip);
        col = func(ColumnWidth[colIndex++]);
        GUI.DrawTexture(col.LeftPartPixels(24f), RS.BondIcon);
        TooltipHandler.TipRegion(col, hasBondsTip);
        col = func(ColumnWidth[colIndex++]);
        GUI.DrawTexture(col.LeftPartPixels(24f), RS.PregnantIcon);
        TooltipHandler.TipRegion(col, pregnancyTip);
        col = func(ColumnWidth[colIndex++]);
        GUI.DrawTexture(col.LeftPartPixels(24f), RS.TrainedIcon);
        TooltipHandler.TipRegion(col, trainedTip);
        col = func(ColumnWidth[colIndex++]);
        GUI.DrawTexture(col.LeftPartPixels(24f), RS.MaleIcon);
        GUI.DrawTexture(col.LeftPartPixels(48f).RightPartPixels(24f), RS.YoungIcon);
        TooltipHandler.TipRegion(col, keepMaleChildCountTip);
        col = func(ColumnWidth[colIndex++]);
        GUI.DrawTexture(col.LeftPartPixels(24f), RS.FemaleIcon);
        GUI.DrawTexture(col.LeftPartPixels(48f).RightPartPixels(24f), RS.YoungIcon);
        TooltipHandler.TipRegion(col, keepFemaleChildCountTip);
        col = func(ColumnWidth[colIndex++]);
        GUI.DrawTexture(col.LeftPartPixels(24f), RS.MaleIcon);
        GUI.DrawTexture(col.LeftPartPixels(48f).RightPartPixels(24f), RS.AdultIcon);
        TooltipHandler.TipRegion(col, keepMaleAdultCountTip);
        col = func(ColumnWidth[colIndex++]);
        GUI.DrawTexture(col.LeftPartPixels(24f), RS.FemaleIcon);
        GUI.DrawTexture(col.LeftPartPixels(48f).RightPartPixels(24f), RS.AdultIcon);
        TooltipHandler.TipRegion(col, keepFemaleAdultCountTip);
        var rect4 = listing_Standard.GetRect(rect.height - listing_Standard.CurHeight);
        var rect5 = new Rect(rect4.x, rect4.y, rect4.width - 30f, defs.Count * 36f);
        Widgets.BeginScrollView(rect4, ref scrollPosition, rect5);
        var list = new Listing_Standard();
        list.Begin(rect5);
        defs.ForEach(delegate(ThingDef d)
        {
            list.GapLine();
            var rect6 = list.GetRect(24f);
            Machine.Settings.TryGetValue(d, out var value);
            if (value == null)
            {
                value = new SlaughterSettings
                {
                    def = d
                };
                Machine.Settings[d] = value;
            }

            var func2 = CutLeftFunc(rect6);
            colIndex = 0;
            col = func2(ColumnWidth[colIndex++]);
            Widgets.Label(col, value.def.label);
            col = func2(ColumnWidth[colIndex++]);
            Widgets.Checkbox(col.position, ref value.doSlaughter);
            TooltipHandler.TipRegion(col, slaughterTip);
            col = func2(ColumnWidth[colIndex++]);
            Widgets.Checkbox(col.position, ref value.hasBonds, 24f, !value.doSlaughter);
            TooltipHandler.TipRegion(col, hasBondsTip);
            col = func2(ColumnWidth[colIndex++]);
            Widgets.Checkbox(col.position, ref value.pregnancy, 24f, !value.doSlaughter);
            TooltipHandler.TipRegion(col, pregnancyTip);
            col = func2(ColumnWidth[colIndex++]);
            Widgets.Checkbox(col.position, ref value.trained, 24f, !value.doSlaughter);
            TooltipHandler.TipRegion(col, trainedTip);
            col = func2(ColumnWidth[colIndex++]);
            var buffer = value.keepMaleYoungCount.ToString();
            Widgets.TextFieldNumeric(col, ref value.keepMaleYoungCount, ref buffer, 0f, 1000f);
            TooltipHandler.TipRegion(col, keepMaleChildCountTip);
            col = func2(ColumnWidth[colIndex++]);
            var buffer2 = value.keepFemaleYoungCount.ToString();
            Widgets.TextFieldNumeric(col, ref value.keepFemaleYoungCount, ref buffer2, 0f, 1000f);
            TooltipHandler.TipRegion(col, keepFemaleChildCountTip);
            col = func2(ColumnWidth[colIndex++]);
            var buffer3 = value.keepMaleAdultCount.ToString();
            Widgets.TextFieldNumeric(col, ref value.keepMaleAdultCount, ref buffer3, 0f, 1000f);
            TooltipHandler.TipRegion(col, keepMaleAdultCountTip);
            col = func2(ColumnWidth[colIndex++]);
            var buffer4 = value.keepFemaleAdultCount.ToString();
            Widgets.TextFieldNumeric(col, ref value.keepFemaleAdultCount, ref buffer4, 0f, 1000f);
            TooltipHandler.TipRegion(col, keepFemaleAdultCountTip);
        });
        list.End();
        Widgets.EndScrollView();
        listing_Standard.End();
    }
}