using System.Collections.Generic;
using LudeonTK;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace NR_AutoMachineTool;

public class ITab_Bills2 : ITab
{
    private static readonly Vector2 WinSize = new Vector2(420f, 480f);

    [TweakValue("Interface", 0f, 128f)] private static readonly float PasteX = 48f;

    [TweakValue("Interface", 0f, 128f)] private static readonly float PasteY = 3f;

    [TweakValue("Interface", 0f, 32f)] private static readonly float PasteSize = 24f;

    private Bill mouseoverBill;

    private Vector2 scrollPosition;
    private float viewHeight = 1000f;

    public ITab_Bills2()
    {
        size = WinSize;
        labelKey = "TabBills";
        tutorTag = "Bills";
    }

    protected ITabBillTable SelTable => (ITabBillTable)SelThing;

    public override void FillTab()
    {
        PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.BillsTab, KnowledgeAmount.FrameDisplayed);
        var rect = new Rect(WinSize.x - PasteX, PasteY, PasteSize, PasteSize);
        if (BillUtility.Clipboard == null)
        {
            GUI.color = Color.gray;
            Widgets.DrawTextureFitted(rect, TexButton.Paste, 1f);
            GUI.color = Color.white;
            TooltipHandler.TipRegionByKey(rect, "PasteBillTip");
        }
        else if (!SelTable.def.AllRecipes.Contains(BillUtility.Clipboard.recipe) ||
                 !BillUtility.Clipboard.recipe.AvailableNow)
        {
            GUI.color = Color.gray;
            Widgets.DrawTextureFitted(rect, TexButton.Paste, 1f);
            GUI.color = Color.white;
            TooltipHandler.TipRegionByKey(rect, "ClipboardBillNotAvailableHere");
        }
        else if (SelTable.billStack.Count >= 15)
        {
            GUI.color = Color.gray;
            Widgets.DrawTextureFitted(rect, TexButton.Paste, 1f);
            GUI.color = Color.white;
            if (Mouse.IsOver(rect))
            {
                TooltipHandler.TipRegion(rect,
                    "PasteBillTip".Translate() + " (" + "PasteBillTip_LimitReached".Translate() + ")");
            }
        }
        else
        {
            if (Widgets.ButtonImageFitted(rect, TexButton.Paste, Color.white))
            {
                var bill = BillUtility.Clipboard.Clone();
                bill.InitializeAfterClone();
                SelTable.billStack.AddBill(bill);
                SoundDefOf.Tick_Low.PlayOneShotOnCamera();
            }

            TooltipHandler.TipRegionByKey(rect, "PasteBillTip");
        }

        var rect2 = new Rect(0f, 0f, WinSize.x, WinSize.y).ContractedBy(10f);

        mouseoverBill = SelTable.billStack.DoListing(rect2, RecipeOptionsMaker, ref scrollPosition, ref viewHeight);
        return;

        List<FloatMenuOption> RecipeOptionsMaker()
        {
            var list = new List<FloatMenuOption>();
            foreach (var recipe in SelTable.AllRecipes)
            {
                if (!recipe.AvailableNow)
                {
                    continue;
                }

                var deletable = SelTable.IsRemovable(recipe);
                list.Add(new FloatMenuOption(recipe.LabelCap, delegate
                {
                    if (!SelTable.Map.mapPawns.FreeColonists.Any(col => recipe.PawnSatisfiesSkillRequirements(col)))
                    {
                        Bill.CreateNoPawnsWithSkillDialog(recipe);
                    }

                    var bill2 = SelTable.MakeNewBill(recipe);
                    SelTable.billStack.AddBill(bill2);
                    if (recipe.conceptLearned != null)
                    {
                        PlayerKnowledgeDatabase.KnowledgeDemonstrated(recipe.conceptLearned, KnowledgeAmount.Total);
                    }

                    if (TutorSystem.TutorialMode)
                    {
                        TutorSystem.Notify_Event("AddBill-" + recipe.LabelCap.Resolve());
                    }
                }, MenuOptionPriority.Default, null, null, deletable ? 58f : 29f, delegate(Rect r)
                {
                    if (!deletable || !Widgets.ButtonImage(new Rect(r.x + 34f, r.y + (r.height - 24f), 24f, 24f),
                            RS.DeleteX))
                    {
                        return Widgets.InfoCardButton(r.x + 5f, r.y + ((r.height - 24f) / 2f), recipe);
                    }

                    SelTable.RemoveRecipe(recipe);
                    return true;
                }));
            }

            if (!list.Any())
            {
                list.Add(new FloatMenuOption("NoneBrackets".Translate(), null));
            }

            return list;
        }
    }

    public override void TabUpdate()
    {
        if (mouseoverBill == null)
        {
            return;
        }

        mouseoverBill.TryDrawIngredientSearchRadiusOnMap(SelTable.Position);
        mouseoverBill = null;
    }
}