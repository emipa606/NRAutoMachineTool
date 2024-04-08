using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace NR_AutoMachineTool;

internal class ITab_PullerFilter : ITab
{
    private static readonly Vector2 WinSize = new Vector2(300f, 500f);

    private readonly string description;

    private readonly ThingFilterUI.UIState uistate = new ThingFilterUI.UIState();

    private List<SlotGroup> groups;

    public ITab_PullerFilter()
    {
        size = WinSize;
        labelKey = "NR_AutoMachineTool_Puller.OutputItemFilterTab";
        description = "NR_AutoMachineTool_Puller.OutputItemFilterText".Translate();
    }

    private Building_ItemPuller Puller => (Building_ItemPuller)SelThing;

    public override bool IsVisible => Puller.Filter != null;

    public override void OnOpen()
    {
        base.OnOpen();
        groups = Puller.Map.haulDestinationManager.AllGroups.ToList();
    }

    public override void FillTab()
    {
        var listing_Standard = new Listing_Standard();
        var rect = new Rect(0f, 0f, WinSize.x, WinSize.y).ContractedBy(10f);
        listing_Standard.Begin(rect);
        listing_Standard.Gap();
        Widgets.Label(listing_Standard.GetRect(40f), description);
        listing_Standard.Gap();
        if (Widgets.ButtonText(listing_Standard.GetRect(30f), "NR_AutoMachineTool_Puller.FilterCopyFrom".Translate()))
        {
            Find.WindowStack.Add(new FloatMenu(groups.Select(g => new FloatMenuOption(g.parent.SlotYielderLabel(),
                delegate { Puller.Filter.CopyAllowancesFrom(g.Settings.filter); })).ToList()));
        }

        listing_Standard.Gap();
        listing_Standard.End();
        var curHeight = listing_Standard.CurHeight;
        ThingFilterUI.DoThingFilterConfigWindow(rect.BottomPartPixels(rect.height - curHeight), uistate, Puller.Filter);
    }
}