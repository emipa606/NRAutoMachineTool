using System.Collections.Generic;
using System.Linq;
using NR_AutoMachineTool.Utilities;
using RimWorld;
using UnityEngine;
using Verse;

namespace NR_AutoMachineTool;

internal class ITab_ProductLimitation : ITab
{
    private static readonly Vector2 WinSize = new(400f, 240f);

    private List<SlotGroup> groups;

    public ITab_ProductLimitation()
    {
        size = WinSize;
        labelKey = "NR_AutoMachineTool.ProductLimitation.TabName";
    }

    private IProductLimitation Machine => (IProductLimitation)SelThing;

    public override void OnOpen()
    {
        base.OnOpen();
        groups = Find.CurrentMap.haulDestinationManager.AllGroups.ToList();
        Machine.TargetSlotGroup = Machine.TargetSlotGroup.Where(s => groups.Contains(s));
    }

    public override void FillTab()
    {
        var label = "NR_AutoMachineTool.ProductLimitation.Description".Translate();
        var label2 = "NR_AutoMachineTool.ProductLimitation.ValueLabel".Translate();
        var taggedString = "NR_AutoMachineTool.ProductLimitation.CheckBoxLabel".Translate();
        var listing_Standard = new Listing_Standard();
        var rect = new Rect(0f, 0f, WinSize.x, WinSize.y).ContractedBy(10f);
        listing_Standard.Begin(rect);
        listing_Standard.Gap();
        Widgets.Label(listing_Standard.GetRect(70f), label);
        listing_Standard.Gap();
        var rect2 = listing_Standard.GetRect(30f);
        var checkOn = Machine.ProductLimitation;
        Widgets.CheckboxLabeled(rect2, taggedString, ref checkOn);
        Machine.ProductLimitation = checkOn;
        listing_Standard.Gap();
        var rect3 = listing_Standard.GetRect(30f);
        var buffer = Machine.ProductLimitCount.ToString();
        var val = Machine.ProductLimitCount;
        Widgets.Label(rect3.LeftHalf(), label2);
        Widgets.TextFieldNumeric(rect3.RightHalf(), ref val, ref buffer, 1f, 1000000f);
        listing_Standard.Gap();
        var rect4 = listing_Standard.GetRect(30f);
        Widgets.Label(rect4.LeftHalf(), "NR_AutoMachineTool.CountZone".Translate());
        if (Widgets.ButtonText(rect4.RightHalf(),
                Machine.TargetSlotGroup.Fold("NR_AutoMachineTool.EntierMap".Translate())(s =>
                    s.parent.SlotYielderLabel())))
        {
            Find.WindowStack.Add(new FloatMenu(groups
                .Select(g => new FloatMenuOption(g.parent.SlotYielderLabel(),
                    delegate { Machine.TargetSlotGroup = Ops.Option(g); })).ToList().Head(
                    new FloatMenuOption("NR_AutoMachineTool.EntierMap".Translate(),
                        delegate { Machine.TargetSlotGroup = Ops.Nothing<SlotGroup>(); }))));
        }

        listing_Standard.Gap();
        listing_Standard.End();
        Machine.ProductLimitCount = val;
    }
}