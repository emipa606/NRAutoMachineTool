﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using Verse;
using Verse.Sound;
using UnityEngine;
using NR_AutoMachineTool.Utilities;
using static NR_AutoMachineTool.Utilities.Ops;

namespace NR_AutoMachineTool
{
    [StaticConstructorOnStartup]
    internal static class TexButton
    {
        public static readonly Texture2D Paste = ContentFinder<Texture2D>.Get("UI/Buttons/Paste", true);
    }

    public interface ITabBillTable
    {
        ThingDef def { get; }
        BillStack billStack { get; }
        Map Map { get; }
        IntVec3 Position { get; }
    }

    // TODO:本体更新時に合わせる.
    public class ITab_Bills2 : ITab
    {
        private float viewHeight = 1000f;

        private Vector2 scrollPosition = default(Vector2);

        private Bill mouseoverBill;

        private static readonly Vector2 WinSize = new Vector2(420f, 480f);

        [TweakValue("Interface", 0f, 128f)]
        private static float PasteX = 48f;

        [TweakValue("Interface", 0f, 128f)]
        private static float PasteY = 3f;

        [TweakValue("Interface", 0f, 32f)]
        private static float PasteSize = 24f;

        protected ITabBillTable SelTable
        {
            get
            {
                return (ITabBillTable)base.SelThing;
            }
        }

        public ITab_Bills2()
        {
            this.size = ITab_Bills2.WinSize;
            this.labelKey = "TabBills";
            this.tutorTag = "Bills";
        }

        protected override void FillTab()
        {
            PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.BillsTab, KnowledgeAmount.FrameDisplayed);
            Rect rect = new Rect(ITab_Bills2.WinSize.x - ITab_Bills2.PasteX, ITab_Bills2.PasteY, ITab_Bills2.PasteSize, ITab_Bills2.PasteSize);
            if (BillUtility.Clipboard == null)
            {
                GUI.color = Color.gray;
                Widgets.DrawTextureFitted(rect, TexButton.Paste, 1f);
                GUI.color = Color.white;
                TooltipHandler.TipRegion(rect, "PasteBillTip".Translate());
            }
            else if (!this.SelTable.def.AllRecipes.Contains(BillUtility.Clipboard.recipe) || !BillUtility.Clipboard.recipe.AvailableNow)
            {
                GUI.color = Color.gray;
                Widgets.DrawTextureFitted(rect, TexButton.Paste, 1f);
                GUI.color = Color.white;
                TooltipHandler.TipRegion(rect, "ClipboardBillNotAvailableHere".Translate());
            }
            else if (this.SelTable.billStack.Count >= 15)
            {
                GUI.color = Color.gray;
                Widgets.DrawTextureFitted(rect, TexButton.Paste, 1f);
                GUI.color = Color.white;
                TooltipHandler.TipRegion(rect, "PasteBillTip".Translate() + " (" + "PasteBillTip_LimitReached".Translate() + ")");
            }
            else
            {
                if (Widgets.ButtonImageFitted(rect, TexButton.Paste, Color.white))
                {
                    Bill bill = BillUtility.Clipboard.Clone();
                    bill.InitializeAfterClone();
                    this.SelTable.billStack.AddBill(bill);
                    SoundDefOf.Tick_Low.PlayOneShotOnCamera(null);
                }
                TooltipHandler.TipRegion(rect, "PasteBillTip".Translate());
            }
            Rect rect2 = new Rect(0f, 0f, ITab_Bills2.WinSize.x, ITab_Bills2.WinSize.y).ContractedBy(10f);
            Func<List<FloatMenuOption>> recipeOptionsMaker = delegate
            {
                List<FloatMenuOption> list = new List<FloatMenuOption>();
                for (int i = 0; i < this.SelTable.def.AllRecipes.Count; i++)
                {
                    if (this.SelTable.def.AllRecipes[i].AvailableNow)
                    {
                        RecipeDef recipe = this.SelTable.def.AllRecipes[i];
                        list.Add(new FloatMenuOption(recipe.LabelCap, delegate
                        {
                            if (!this.SelTable.Map.mapPawns.FreeColonists.Any((Pawn col) => recipe.PawnSatisfiesSkillRequirements(col)))
                            {
                                Bill.CreateNoPawnsWithSkillDialog(recipe);
                            }
                            Bill bill2 = recipe.MakeNewBill();
                            this.SelTable.billStack.AddBill(bill2);
                            if (recipe.conceptLearned != null)
                            {
                                PlayerKnowledgeDatabase.KnowledgeDemonstrated(recipe.conceptLearned, KnowledgeAmount.Total);
                            }
                            if (TutorSystem.TutorialMode)
                            {
                                TutorSystem.Notify_Event("AddBill-" + recipe.LabelCap);
                            }
                        }, MenuOptionPriority.Default, null, null, 29f, (Rect r) => Widgets.InfoCardButton(r.x + 5f, r.y + (r.height - 24f) / 2f, recipe), null));
                    }
                }
                if (!list.Any<FloatMenuOption>())
                {
                    list.Add(new FloatMenuOption("NoneBrackets".Translate(), null, MenuOptionPriority.Default, null, null, 0f, null, null));
                }
                return list;
            };
            this.mouseoverBill = this.SelTable.billStack.DoListing(rect2, recipeOptionsMaker, ref this.scrollPosition, ref this.viewHeight);
        }

        public override void TabUpdate()
        {
            if (this.mouseoverBill != null)
            {
                this.mouseoverBill.TryDrawIngredientSearchRadiusOnMap(this.SelTable.Position);
                this.mouseoverBill = null;
            }
        }
    }
}