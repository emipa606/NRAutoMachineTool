using System.Collections.Generic;
using System.Linq;
using NR_AutoMachineTool.Utilities;
using RimWorld;
using UnityEngine;
using Verse;

namespace NR_AutoMachineTool;

internal class ITab_ConveyorFilter : ITab
{
    private static readonly Vector2 WinSize = new Vector2(300f, 500f);

    private static readonly Dictionary<Rot4, string> RotStrings = new Dictionary<Rot4, string>
    {
        {
            Rot4.North,
            "N"
        },
        {
            Rot4.East,
            "E"
        },
        {
            Rot4.South,
            "S"
        },
        {
            Rot4.West,
            "W"
        }
    };

    private readonly string description;

    private readonly Dictionary<Building_BeltConveyor, Dictionary<Rot4, bool>> rotSelectedDic =
        new Dictionary<Building_BeltConveyor, Dictionary<Rot4, bool>>();

    private readonly ThingFilterUI.UIState uistate = new ThingFilterUI.UIState();

    private List<SlotGroup> groups;

    public ITab_ConveyorFilter()
    {
        size = WinSize;
        labelKey = "NR_AutoMachineTool_Conveyor.OutputItemFilterTab";
        description = "NR_AutoMachineTool_Conveyor.OutputItemFilterText".Translate();
    }

    private Building_BeltConveyor Conveyor => (Building_BeltConveyor)SelThing;

    public override bool IsVisible => Conveyor.Filters.Count > 1;

    public override void OnOpen()
    {
        base.OnOpen();
        groups = Conveyor.Map.haulDestinationManager.AllGroups.ToList();
    }

    public override void FillTab()
    {
        if (!rotSelectedDic.ContainsKey(Conveyor))
        {
            var dictionary = Enumerable.Range(0, 4).ToDictionary(k => new Rot4(k), _ => false);
            dictionary[Conveyor.Filters.First().Key] = true;
            rotSelectedDic[Conveyor] = dictionary;
        }

        var dic = rotSelectedDic[Conveyor];
        if (!Conveyor.Filters.ContainsKey(dic.First(kv => kv.Value).Key))
        {
            new Dictionary<Rot4, bool>(dic).ForEach(delegate(KeyValuePair<Rot4, bool> x) { dic[x.Key] = false; });
            dic[Conveyor.Filters.First().Key] = true;
        }

        var listing_Standard = new Listing_Standard();
        var rect = new Rect(0f, 0f, WinSize.x, WinSize.y).ContractedBy(10f);
        listing_Standard.Begin(rect);
        listing_Standard.Gap();
        var rect2 = listing_Standard.GetRect(40f);
        Widgets.Label(rect2, description);
        listing_Standard.Gap();
        var pos = new Dictionary<Rot4, Rect>();
        rect2 = listing_Standard.GetRect(30f);
        pos[Rot4.North] = new Rect(rect2.x + (rect2.width / 4f), rect2.y, rect2.width / 2f, rect2.height);
        rect2 = listing_Standard.GetRect(30f);
        pos[Rot4.West] = rect2.LeftHalf();
        pos[Rot4.East] = rect2.RightHalf();
        rect2 = listing_Standard.GetRect(30f);
        pos[Rot4.South] = new Rect(rect2.x + (rect2.width / 4f), rect2.y, rect2.width / 2f, rect2.height);
        new Dictionary<Rot4, bool>(dic).ForEach(delegate(KeyValuePair<Rot4, bool> kv)
        {
            var label = ("NR_AutoMachineTool.OutputDirection" + RotStrings[kv.Key]).Translate();
            Text.Anchor = TextAnchor.MiddleRight;
            Widgets.Label(pos[kv.Key].LeftHalf(), label);
            if (Conveyor.Filters.ContainsKey(kv.Key))
            {
                if (!Widgets.RadioButton(pos[kv.Key].RightHalf().position, kv.Value))
                {
                    return;
                }

                new Dictionary<Rot4, bool>(dic).ForEach(delegate(KeyValuePair<Rot4, bool> x) { dic[x.Key] = false; });
                dic[kv.Key] = true;
            }
            else
            {
                Text.Anchor = TextAnchor.MiddleLeft;
                var font = Text.Font;
                Text.Font = GameFont.Tiny;
                Widgets.Label(pos[kv.Key].RightHalf(),
                    "NR_AutoMachineTool_Conveyor.OutputItemFilterNotOutputDestination".Translate());
                Text.Font = font;
            }
        });
        listing_Standard.Gap();
        var option = (from kv in dic
            where kv.Value
            select kv.Key).FirstOption();
        if (!option.HasValue)
        {
            listing_Standard.End();
            return;
        }

        var selectedRot = option.Value;
        rect2 = listing_Standard.GetRect(30f);
        Widgets.Label(rect2.LeftHalf(), "NR_AutoMachineTool.Priority".Translate());
        if (Widgets.ButtonText(rect2.RightHalf(), Conveyor.Priorities[selectedRot].ToText()))
        {
            Find.WindowStack.Add(new FloatMenu((from k in Ops.GetEnumValues<DirectionPriority>()
                orderby (int)k descending
                select k
                into d
                select new FloatMenuOption(d.ToText(), delegate { Conveyor.Priorities[selectedRot] = d; })).ToList()));
        }

        listing_Standard.Gap();
        rect2 = listing_Standard.GetRect(30f);
        if (Widgets.ButtonText(rect2, "NR_AutoMachineTool_Puller.FilterCopyFrom".Translate()))
        {
            Find.WindowStack.Add(new FloatMenu(groups.Select(g => new FloatMenuOption(g.parent.SlotYielderLabel(),
                delegate { Conveyor.Filters[selectedRot].CopyAllowancesFrom(g.Settings.filter); })).ToList()));
        }

        listing_Standard.Gap();
        listing_Standard.End();
        var curHeight = listing_Standard.CurHeight;
        ThingFilterUI.DoThingFilterConfigWindow(rect.BottomPartPixels(rect.height - curHeight), uistate,
            Conveyor.Filters[selectedRot]);
    }
}