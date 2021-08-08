﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using Verse;
using Verse.AI;
using UnityEngine;
using NR_AutoMachineTool.Utilities;
using static NR_AutoMachineTool.Utilities.Ops;
using static Verse.ThingFilterUI;

namespace NR_AutoMachineTool
{
    interface IThingFilter
    {
        ThingFilter Filter { get; }
        Map Map { get; }
        int? Count { get; set; }
    }

    class ITab_ThingFilter : ITab
    {
        private static readonly Vector2 WinSize = new Vector2(300f, 500f);

        public ITab_ThingFilter()
        {
            this.size = WinSize;
            this.labelKey = "NR_AutoMachineTool_Puller.OutputItemFilterTab";

            this.description = "NR_AutoMachineTool_Puller.OutputItemFilterText".Translate();
        }

        private string description;

        private IThingFilter Machine
        {
            get => (IThingFilter)this.SelThing;
        }

        public override void OnOpen()
        {
            base.OnOpen();

            this.groups = this.Machine.Map.haulDestinationManager.AllGroups.ToList();
        }

        private List<SlotGroup> groups;

        public override bool IsVisible => Machine.Filter != null;
        
        private Vector2 scrollPosition;

        protected override void FillTab()
        {
            Listing_Standard list = new Listing_Standard();
            Rect inRect = new Rect(0f, 0f, WinSize.x, WinSize.y).ContractedBy(10f);

            list.Begin(inRect);
            list.Gap();

            var rect = list.GetRect(40f);
            Widgets.Label(rect, this.description);
            list.Gap();

            rect = list.GetRect(30f);
            if (Widgets.ButtonText(rect, "NR_AutoMachineTool_Puller.FilterCopyFrom".Translate()))
            {
                Find.WindowStack.Add(new FloatMenu(groups.Select(g => new FloatMenuOption(g.parent.SlotYielderLabel(), () => this.Machine.Filter.CopyAllowancesFrom(g.Settings.filter))).ToList()));
            }
            list.Gap();

            if(this.Machine.Count.HasValue)
            {
                rect = list.GetRect(30f);
                int count = this.Machine.Count.Value;
                string buf = null;
                Widgets.TextFieldNumericLabeled<int>(rect, "NR_AutoMachineTool.Count".Translate(), ref count, ref buf, 1, 100000);
                this.Machine.Count = count;
                list.Gap();
            }

            list.End();
            var height = list.CurHeight;
            UIState uistate = new UIState();
            uistate.scrollPosition = this.scrollPosition;
            ThingFilterUI.DoThingFilterConfigWindow(inRect.BottomPartPixels(inRect.height - height), uistate, this.Machine.Filter);

        }
    }
}
