using System.Collections.Generic;
using System.Linq;
using NR_AutoMachineTool.Utilities;
using RimWorld;
using Verse;

namespace NR_AutoMachineTool;

public class Building_Miner : Building_BaseMachine<Building_Miner>, IBillGiver, IRecipeProductWorker, ITabBillTable
{
    private readonly IntVec3[] adjacent =
    {
        new IntVec3(0, 0, 1),
        new IntVec3(1, 0, 1),
        new IntVec3(1, 0, 0),
        new IntVec3(1, 0, -1),
        new IntVec3(0, 0, -1),
        new IntVec3(-1, 0, -1),
        new IntVec3(-1, 0, 0),
        new IntVec3(-1, 0, 1)
    };

    private string[] adjacentName = { "N", "NE", "E", "SE", "S", "SW", "W", "NW" };
    public BillStack billStack;

    private int outputIndex;

    private Bill workingBill;

    [Unsaved] private Option<Effecter> workingEffect = Ops.Nothing<Effecter>();

    public Building_Miner()
    {
        billStack = new BillStack(this);
        forcePlace = false;
    }

    private ModExtension_AutoMachineTool Extension => def.GetModExtension<ModExtension_AutoMachineTool>();

    protected override float SpeedFactor => Setting.minerSetting.speedFactor;

    public override int MinPowerForSpeed => Setting.minerSetting.minSupplyPowerForSpeed;

    public override int MaxPowerForSpeed => Setting.minerSetting.maxSupplyPowerForSpeed;

    public BillStack BillStack => billStack;

    public IEnumerable<IntVec3> IngredientStackCells => Enumerable.Empty<IntVec3>();

    public bool CurrentlyUsableForBills()
    {
        return false;
    }

    public bool UsableForBillsAfterFueling()
    {
        return false;
    }

    public void Notify_BillDeleted(Bill bill)
    {
    }

    public Room GetRoom(RegionType type)
    {
        return RegionAndRoomQuery.GetRoom(this, type);
    }

    public int GetSkillLevel(SkillDef def)
    {
        return 20;
    }

    ThingDef ITabBillTable.def => def;

    BillStack ITabBillTable.billStack => BillStack;

    public IEnumerable<RecipeDef> AllRecipes => def.AllRecipes;

    public bool IsRemovable(RecipeDef recipe)
    {
        return false;
    }

    public void RemoveRecipe(RecipeDef recipe)
    {
    }

    public Bill MakeNewBill(RecipeDef recipe)
    {
        return recipe.MakeNewBill();
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref outputIndex, "outputIndex");
        Scribe_Deep.Look(ref billStack, "billStack", this);
        Scribe_References.Look(ref workingBill, "workingBill");
        if (Scribe.mode == LoadSaveMode.PostLoadInit && workingBill == null)
        {
            readyOnStart = true;
        }
    }

    protected override bool WorkInterruption(Building_Miner working)
    {
        return !workingBill.ShouldDoNow();
    }

    protected override bool TryStartWorking(out Building_Miner target, out float workAmount)
    {
        target = this;
        workAmount = 0f;
        if (!billStack.AnyShouldDoNow)
        {
            return false;
        }

        workingBill = billStack.FirstShouldDoNow;
        workAmount = workingBill.recipe.workAmount;
        return true;
    }

    protected override bool FinishWorking(Building_Miner working, out List<Thing> products)
    {
        products = GenRecipe2.MakeRecipeProducts(workingBill.recipe, this, [], null, this).ToList();
        workingBill.Notify_IterationCompleted(null, []);
        workingBill = null;
        return true;
    }

    protected override void CleanupWorkingEffect()
    {
        base.CleanupWorkingEffect();
        workingEffect.ForEach(delegate(Effecter e) { e.Cleanup(); });
        workingEffect = Ops.Nothing<Effecter>();
        MapManager.RemoveEachTickAction(EffectTick);
    }

    protected override void CreateWorkingEffect()
    {
        base.CreateWorkingEffect();
        workingEffect = workingEffect.Fold(() => from e in Ops.Option(workingBill.recipe.effectWorking)
            select e.Spawn())(Ops.Option);
        MapManager.EachTickAction(EffectTick);
    }

    protected bool EffectTick()
    {
        workingEffect.ForEach(delegate(Effecter e) { e.EffectTick(new TargetInfo(this), new TargetInfo(this)); });
        return !workingEffect.HasValue;
    }

    public override IEnumerable<Gizmo> GetGizmos()
    {
        foreach (var gizmo in base.GetGizmos())
        {
            yield return gizmo;
        }

        var command_Action = new Command_Action
        {
            action = delegate
            {
                if (outputIndex + 1 >= adjacent.Length)
                {
                    outputIndex = 0;
                }
                else
                {
                    outputIndex++;
                }
            },
            activateSound = SoundDefOf.Checkbox_TurnedOn,
            defaultLabel = "NR_AutoMachineTool.SelectOutputDirectionLabel".Translate(),
            defaultDesc = "NR_AutoMachineTool.SelectOutputDirectionDesc".Translate(),
            icon = RS.OutputDirectionIcon
        };
        yield return command_Action;
    }

    public override IntVec3 OutputCell()
    {
        return Position + adjacent[outputIndex];
    }
}