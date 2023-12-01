using System;
using System.Collections.Generic;
using System.Linq;
using NR_AutoMachineTool.Utilities;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace NR_AutoMachineTool;

public class Building_AutoMachineTool : Building_BaseRange<Building_AutoMachineTool>, IRecipeProductWorker
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

    private readonly string[] adjacentName = { "N", "NE", "E", "SE", "S", "SW", "W", "NW" };

    private Bill bill;

    private Thing dominant;

    private bool forbidItem;

    private List<Thing> ingredients;

    private int outputIndex;

    private UnfinishedThing unfinished;

    [Unsaved] private Option<Effecter> workingEffect = Ops.Nothing<Effecter>();

    [Unsaved] private Option<Sustainer> workingSound = Ops.Nothing<Sustainer>();

    [Unsaved] private Option<Building_WorkTable> workTable;

    public Building_AutoMachineTool()
    {
        forcePlace = false;
        targetEnumrationCount = 0;
    }

    private Map M => Map;

    private IntVec3 P => Position;

    protected override int? SkillLevel => Setting.AutoMachineToolTier(Extension.tier).skillLevel;

    public override int MaxPowerForSpeed => Setting.AutoMachineToolTier(Extension.tier).maxSupplyPowerForSpeed;

    public override int MinPowerForSpeed => Setting.AutoMachineToolTier(Extension.tier).minSupplyPowerForSpeed;

    protected override float SpeedFactor => Setting.AutoMachineToolTier(Extension.tier).speedFactor;

    public override bool Glowable => false;

    public int GetSkillLevel(SkillDef def)
    {
        return SkillLevel.GetValueOrDefault();
    }

    public Room GetRoom(RegionType type)
    {
        return RegionAndRoomQuery.GetRoom(this, type);
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref outputIndex, "outputIndex");
        Scribe_Values.Look(ref forbidItem, "forbidItem");
        Scribe_Deep.Look(ref unfinished, "unfinished");
        Scribe_References.Look(ref bill, "bill");
        Scribe_References.Look(ref dominant, "dominant");
        Scribe_Collections.Look(ref ingredients, "ingredients", LookMode.Deep);
    }

    public override void SpawnSetup(Map map, bool respawningAfterLoad)
    {
        base.SpawnSetup(map, respawningAfterLoad);
        workTable = Ops.Nothing<Building_WorkTable>();
    }

    public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
    {
        workTable.ForEach(AllowWorkTable);
        base.DeSpawn(mode);
    }

    public override void PostMapInit()
    {
        base.PostMapInit();
        WorkTableSetting();
    }

    protected override void Reset()
    {
        if (State == WorkingState.Working)
        {
            if (unfinished == null)
            {
                ingredients.ForEach(delegate(Thing t) { GenPlace.TryPlaceThing(t, P, M, ThingPlaceMode.Near); });
            }
            else
            {
                GenPlace.TryPlaceThing(unfinished, P, M, ThingPlaceMode.Near);
                unfinished.Destroy(DestroyMode.FailConstruction);
            }
        }

        bill = null;
        dominant = null;
        unfinished = null;
        ingredients = null;
        base.Reset();
    }

    protected override void CleanupWorkingEffect()
    {
        base.CleanupWorkingEffect();
        workingEffect.ForEach(delegate(Effecter e) { e.Cleanup(); });
        workingEffect = Ops.Nothing<Effecter>();
        workingSound.ForEach(delegate(Sustainer s) { s.End(); });
        workingSound = Ops.Nothing<Sustainer>();
        MapManager.RemoveEachTickAction(EffectTick);
    }

    protected override void CreateWorkingEffect()
    {
        base.CreateWorkingEffect();
        workingEffect = workingEffect.Fold(() => from e in Ops.Option(bill.recipe.effectWorking)
            select e.Spawn())(Ops.Option);
        workingSound = workingSound.Fold(() => workTable.SelectMany(t => from s in Ops.Option(bill.recipe.soundWorking)
            select s.TrySpawnSustainer(t)))(Ops.Option).Peek(delegate(Sustainer s) { s.Maintain(); });
        MapManager.EachTickAction(EffectTick);
    }

    protected bool EffectTick()
    {
        workingEffect.ForEach(delegate(Effecter e)
        {
            workTable.ForEach(delegate(Building_WorkTable w)
            {
                e.EffectTick(new TargetInfo(this), new TargetInfo(w));
            });
        });
        return !workingEffect.HasValue;
    }

    private void ForbidWorkTable(Building_WorkTable worktable)
    {
        ForbidBills(worktable);
    }

    private void AllowWorkTable(Building_WorkTable worktable)
    {
        AllowBills(worktable);
    }

    private void ForbidBills(Building_WorkTable worktable)
    {
        if (!worktable.BillStack.Bills.Any(b => !(b is IBill_PawnForbidded)))
        {
            return;
        }

        var source = worktable.BillStack.Bills.ToList();
        worktable.BillStack.Clear();
        worktable.BillStack.Bills.AddRange(source.SelectMany(delegate(Bill b)
        {
            var bill_PawnForbidded = b as IBill_PawnForbidded;
            if (bill_PawnForbidded != null)
            {
                return Ops.Option((Bill)bill_PawnForbidded);
            }

            if (b is Bill_ProductionWithUft uft)
            {
                bill_PawnForbidded = uft.CopyTo(
                    (Bill_ProductionWithUftPawnForbidded)Activator.CreateInstance(
                        typeof(Bill_ProductionWithUftPawnForbidded), uft.recipe));
                uft.repeatMode = BillRepeatModeDefOf.Forever;
                bill_PawnForbidded.Original = uft;
            }
            else if (b is Bill_Production production)
            {
                bill_PawnForbidded = production.CopyTo(
                    (Bill_ProductionPawnForbidded)Activator.CreateInstance(typeof(Bill_ProductionPawnForbidded),
                        production.recipe));
                production.repeatMode = BillRepeatModeDefOf.Forever;
                bill_PawnForbidded.Original = production;
            }

            return Ops.Option((Bill)bill_PawnForbidded);
        }));
    }

    private void AllowBills(Building_WorkTable worktable)
    {
        if (!worktable.BillStack.Bills.Any(b => b is IBill_PawnForbidded))
        {
            return;
        }

        var source = worktable.BillStack.Bills.ToList();
        worktable.BillStack.Clear();
        worktable.BillStack.Bills.AddRange(source.SelectMany(delegate(Bill b)
        {
            var bill_PawnForbidded = b as IBill_PawnForbidded;
            var value = b;
            if (bill_PawnForbidded == null)
            {
                return Ops.Option(value);
            }

            if (b is Bill_ProductionWithUft uft)
            {
                value = uft.CopyTo(
                    (Bill_ProductionWithUft)Activator.CreateInstance(
                        bill_PawnForbidded.Original?.GetType() ?? typeof(Bill_ProductionWithUft), uft.recipe));
            }
            else if (b is Bill_Production production)
            {
                value = production.CopyTo(
                    (Bill_Production)Activator.CreateInstance(
                        bill_PawnForbidded.Original?.GetType() ?? typeof(Bill_Production), production.recipe));
            }

            return Ops.Option(value);
        }));
    }

    protected override TargetInfo ProgressBarTarget()
    {
        return workTable.GetOrDefault(null);
    }

    private void WorkTableSetting()
    {
        var targetWorkTable = GetTargetWorkTable();
        if (workTable.HasValue && !targetWorkTable.HasValue)
        {
            AllowWorkTable(workTable.Value);
        }

        targetWorkTable.ForEach(ForbidWorkTable);
        workTable = targetWorkTable;
    }

    protected override void Ready()
    {
        WorkTableSetting();
        base.Ready();
    }

    private IntVec3 FacingCell()
    {
        return Position + Rotation.FacingCell;
    }

    private Option<Building_WorkTable> GetTargetWorkTable()
    {
        return (from t in (from t in FacingCell().GetThingList(M)
                where t.def.category == ThingCategory.Building
                select t).SelectMany(t => Ops.Option(t as Building_WorkTable))
            where t.InteractionCell == Position
            select t).FirstOption();
    }

    protected override bool TryStartWorking(out Building_AutoMachineTool target, out float workAmount)
    {
        target = this;
        workAmount = 0f;
        if (!workTable.Where(t => t.CurrentlyUsableForBills() && t.billStack.AnyShouldDoNow).HasValue)
        {
            return false;
        }

        var consumable = Consumable();
        var orDefault = WorkableBill(consumable).Select(delegate(Utilities.Tuple<Bill, List<ThingAmount>> tuple)
        {
            bill = tuple.Value1;
            ingredients = tuple.Value2.Select(t => t.thing.SplitOff(t.count)).ToList();
            dominant = DominantIngredient(ingredients);
            if (!bill.recipe.UsesUnfinishedThing)
            {
                return new
                {
                    Result = true,
                    WorkAmount = bill.recipe.WorkAmountTotal(!bill.recipe.UsesUnfinishedThing ? null : dominant?.def)
                };
            }

            var stuff = !bill.recipe.unfinishedThingDef.MadeFromStuff ? null : dominant.def;
            unfinished = (UnfinishedThing)ThingMaker.MakeThing(bill.recipe.unfinishedThingDef, stuff);
            unfinished.BoundBill = (Bill_ProductionWithUft)bill;
            unfinished.ingredients = ingredients;
            unfinished.TryGetComp<CompColorable>()?.SetColor(dominant.DrawColor);

            return new
            {
                Result = true,
                WorkAmount = bill.recipe.WorkAmountTotal(!bill.recipe.UsesUnfinishedThing ? null : dominant?.def)
            };
        }).GetOrDefault(new
        {
            Result = false,
            WorkAmount = 0f
        });
        workAmount = orDefault.WorkAmount;
        return orDefault.Result;
    }

    protected override bool FinishWorking(Building_AutoMachineTool working, out List<Thing> products)
    {
        products = GenRecipe2.MakeRecipeProducts(bill.recipe, this, ingredients, dominant, workTable.GetOrDefault(null))
            .ToList();
        ingredients.ForEach(delegate(Thing i) { bill.recipe.Worker.ConsumeIngredient(i, bill.recipe, M); });
        Ops.Option(unfinished).ForEach(delegate(UnfinishedThing u) { u.Destroy(); });
        bill.Notify_IterationCompleted(null, ingredients);
        bill = null;
        dominant = null;
        unfinished = null;
        ingredients = null;
        return true;
    }

    public List<IntVec3> OutputZone()
    {
        return OutputCell().SlotGroupCells(M);
    }

    public override IntVec3 OutputCell()
    {
        return Position + adjacent[outputIndex];
    }

    private List<Thing> Consumable()
    {
        return (from c in GetAllTargetCells().SelectMany(c => c.GetThingList(M))
            where c.def.category == ThingCategory.Item
            select c).ToList();
    }

    private Option<Utilities.Tuple<Bill, List<ThingAmount>>> WorkableBill(List<Thing> consumable)
    {
        return workTable.Where(t => t.CurrentlyUsableForBills()).SelectMany(wt => (from b in wt.billStack.Bills
            where b.ShouldDoNow()
            where b.recipe.AvailableNow
            where Ops.Option(b.recipe.skillRequirements).Fold(true)(s =>
                s.Where(x => x != null).All(r => r.minLevel <= GetSkillLevel(r.skill)))
            select Ops.Tuple(b, Ingredients(b, consumable))
            into t
            where t.Value1.recipe.ingredients.Count == 0 || t.Value2.Count > 0
            select t).FirstOption());
    }

    private List<ThingAmount> Ingredients(Bill bill, List<Thing> consumable)
    {
        var arg = consumable.Select(x => new ThingAmount(x, x.stackCount)).ToList();

        List<ThingDefGroup> Grouping(List<ThingAmount> consumableAmounts)
        {
            return (from c in consumableAmounts
                    group c by c.thing.def
                    into c
                    select new { Def = c.Key, Count = c.Sum(t => t.count), Amounts = c.Select(t => t) }
                    into g
                    orderby g.Def.IsStuff descending, g.Count * bill.recipe.IngredientValueGetter.ValuePerUnitOf(g.Def)
                        descending
                    select g).Select(g =>
                {
                    var result = default(ThingDefGroup);
                    result.def = g.Def;
                    result.consumable = g.Amounts.ToList();
                    return result;
                })
                .ToList();
        }

        var grouped = Grouping(arg);
        var source = bill.recipe.ingredients.Select(delegate(IngredientCount i)
        {
            var list = new List<ThingAmount>();
            var num = i.GetBaseCount();
            foreach (var item in grouped)
            {
                foreach (var item2 in item.consumable)
                {
                    var thing = item2.thing;
                    if (i.filter.Allows(thing) && (bill.ingredientFilter.Allows(thing) || i.IsFixedIngredient))
                    {
                        num -= bill.recipe.IngredientValueGetter.ValuePerUnitOf(thing.def) * item2.count;
                        var num2 = item2.count;
                        if (num <= 0f)
                        {
                            num2 -= Mathf.RoundToInt((0f - num) /
                                                     bill.recipe.IngredientValueGetter.ValuePerUnitOf(thing.def));
                            num = 0f;
                        }

                        list.Add(new ThingAmount(thing, num2));
                    }

                    if (num <= 0f)
                    {
                        break;
                    }
                }

                if (num <= 0f)
                {
                    break;
                }

                if ((!item.def.IsStuff || !bill.recipe.productHasIngredientStuff) && bill.recipe.allowMixingIngredients)
                {
                    continue;
                }

                num = i.GetBaseCount();
                list.Clear();
            }

            if (!(num <= 0f))
            {
                return [];
            }

            var grouped1 = grouped;
            list.ForEach(delegate(ThingAmount r)
            {
                var consumable2 = grouped1.Find(x => x.def == r.thing.def).consumable;
                var thingAmount = consumable2.Find(x => x.thing == r.thing);
                consumable2.Remove(thingAmount);
                thingAmount.count -= r.count;
                consumable2.Add(thingAmount);
            });
            grouped = Grouping(grouped.SelectMany(x => x.consumable).ToList());
            return list;
        }).ToList();
        return source.All(x => x.Count > 0) ? source.SelectMany(c => c).ToList() : [];
    }

    private Thing DominantIngredient(List<Thing> ingredients)
    {
        if (ingredients.Count == 0)
        {
            return null;
        }

        if (bill.recipe.productHasIngredientStuff)
        {
            return ingredients[0];
        }

        return bill.recipe.products.Any(x => x.thingDef.MadeFromStuff)
            ? ingredients.Where(x => x.def.IsStuff).RandomElementByWeight(x => x.stackCount)
            : ingredients.RandomElementByWeight(x => x.stackCount);
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
        var command_Toggle = new Command_Toggle
        {
            isActive = () => forbidItem,
            toggleAction = delegate { forbidItem = !forbidItem; },
            defaultLabel = "NR_AutoMachineTool.ForbidOutputItemLabel".Translate(),
            defaultDesc = "NR_AutoMachineTool.ForbidOutputItemDesc".Translate(),
            icon = RS.ForbidIcon
        };
        yield return command_Toggle;
    }

    public override string GetInspectString()
    {
        return base.GetInspectString() + "\n" +
               "NR_AutoMachineTool.OutputDirection".Translate(
                   ("NR_AutoMachineTool.OutputDirection" + adjacentName[outputIndex]).Translate());
    }

    protected override bool WorkInterruption(Building_AutoMachineTool working)
    {
        if (!workTable.HasValue)
        {
            return true;
        }

        var targetWorkTable = GetTargetWorkTable();
        if (!targetWorkTable.HasValue)
        {
            return true;
        }

        if (targetWorkTable.Value != workTable.Value)
        {
            return true;
        }

        return !workTable.Value.CurrentlyUsableForBills();
    }

    public interface IBill_PawnForbidded
    {
        Bill Original { get; set; }
    }

    public class Bill_ProductionPawnForbidded : Bill_Production, IBill_PawnForbidded
    {
        public Bill original;

        public Bill_ProductionPawnForbidded()
        {
        }

        public Bill_ProductionPawnForbidded(RecipeDef recipe)
            : base(recipe)
        {
        }

        public Bill Original
        {
            get => original;
            set => original = value;
        }

        public override bool PawnAllowedToStartAnew(Pawn p)
        {
            return false;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Deep.Look(ref original, "original");
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                original.billStack = billStack;
            }
        }

        public override Bill Clone()
        {
            var copy = (Bill_Production)original.Clone();
            return this.CopyTo(copy);
        }

        public override void Notify_DoBillStarted(Pawn billDoer)
        {
            base.Notify_DoBillStarted(billDoer);
            Ops.Option(original).ForEach(delegate(Bill o) { o.Notify_DoBillStarted(billDoer); });
        }

        public override void Notify_IterationCompleted(Pawn billDoer, List<Thing> ingredients)
        {
            base.Notify_IterationCompleted(billDoer, ingredients);
            Ops.Option(original).ForEach(delegate(Bill o) { o.Notify_IterationCompleted(billDoer, ingredients); });
        }

        public override void Notify_PawnDidWork(Pawn p)
        {
            base.Notify_PawnDidWork(p);
            Ops.Option(original).ForEach(delegate(Bill o) { o.Notify_PawnDidWork(p); });
        }
    }

    public class Bill_ProductionWithUftPawnForbidded : Bill_ProductionWithUft, IBill_PawnForbidded
    {
        public Bill original;

        public Bill_ProductionWithUftPawnForbidded()
        {
        }

        public Bill_ProductionWithUftPawnForbidded(RecipeDef recipe)
            : base(recipe)
        {
        }

        public Bill Original
        {
            get => original;
            set => original = value;
        }

        public override bool PawnAllowedToStartAnew(Pawn p)
        {
            return false;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Deep.Look(ref original, "original");
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                original.billStack = billStack;
            }
        }

        public override Bill Clone()
        {
            var copy = (Bill_ProductionWithUft)original.Clone();
            return this.CopyTo(copy);
        }

        public override void Notify_DoBillStarted(Pawn billDoer)
        {
            base.Notify_DoBillStarted(billDoer);
            Ops.Option(original).ForEach(delegate(Bill o) { o.Notify_DoBillStarted(billDoer); });
        }

        public override void Notify_IterationCompleted(Pawn billDoer, List<Thing> ingredients)
        {
            base.Notify_IterationCompleted(billDoer, ingredients);
            Ops.Option(original).ForEach(delegate(Bill o) { o.Notify_IterationCompleted(billDoer, ingredients); });
        }

        public override void Notify_PawnDidWork(Pawn p)
        {
            base.Notify_PawnDidWork(p);
            Ops.Option(original).ForEach(delegate(Bill o) { o.Notify_PawnDidWork(p); });
        }
    }

    private struct ThingDefGroup
    {
        public ThingDef def;

        public List<ThingAmount> consumable;
    }

    private class ThingAmount
    {
        public readonly Thing thing;
        public int count;

        public ThingAmount(Thing thing, int count)
        {
            this.thing = thing;
            this.count = count;
        }
    }
}