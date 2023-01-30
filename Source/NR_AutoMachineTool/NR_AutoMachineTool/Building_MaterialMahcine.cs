using System.Collections.Generic;
using System.Linq;
using NR_AutoMachineTool.Utilities;
using RimWorld;
using UnityEngine;
using Verse;

namespace NR_AutoMachineTool;

public class Building_MaterialMahcine : Building_WorkTable, IBillNotificationReceiver, ITabBillTable
{
    private const string ToEnergy10DefName = "NR_AutoMachineTool.ToEnergy10";

    private const string ToEnergy100DefName = "NR_AutoMachineTool.ToEnergy100";

    private const string ScanMaterialDefName = "NR_AutoMachineTool.ScanMaterial";

    private static readonly HashSet<string> ToEnergyDefNames = new HashSet<string>(new[]
        { "NR_AutoMachineTool.ToEnergy10", "NR_AutoMachineTool.ToEnergy100" });

    private static ThingDef energyThingDef;

    private static List<RecipeDef> toEnergyRecipes;

    private static RecipeDef scanMaterialRecipe;

    [Unsaved] private readonly List<RecipeDef> allRecipes = new List<RecipeDef>();

    private List<MaterializeRecipeDefData> materializeRecipeData = new List<MaterializeRecipeDefData>();

    private float Loss => 0.3f;

    public void OnComplete(Bill_Production bill, List<Thing> ingredients)
    {
        if ("NR_AutoMachineTool.ScanMaterial" != bill.recipe.defName)
        {
            return;
        }

        ingredients.ForEach(delegate(Thing i)
        {
            var defName = MaterializeRecipeDefData.GetDefName(i.def, i.Stuff, 1);
            if (materializeRecipeData.Any(r => r.defName == defName))
            {
                return;
            }

            var materializeRecipeDefData = new MaterializeRecipeDefData(i.def, i.Stuff, 1, 300);
            materializeRecipeData.Add(materializeRecipeDefData);
            materializeRecipeDefData.Register(Loss);
            allRecipes.Add(materializeRecipeDefData.GetRecipe());
            if (i.def.stackLimit < 10)
            {
                return;
            }

            materializeRecipeDefData = new MaterializeRecipeDefData(i.def, i.Stuff, 10, 1000);
            materializeRecipeDefData.Register(Loss);
            materializeRecipeData.Add(materializeRecipeDefData);
            allRecipes.Add(materializeRecipeDefData.GetRecipe());
        });
    }

    ThingDef ITabBillTable.def => def;

    BillStack ITabBillTable.billStack => billStack;

    public IEnumerable<RecipeDef> AllRecipes => allRecipes;

    public bool IsRemovable(RecipeDef recipe)
    {
        return recipe.defName.StartsWith("NR_AutoMachineTool.Materialize_");
    }

    public void RemoveRecipe(RecipeDef recipe)
    {
        billStack.Bills.Where(b => b.recipe.defName == recipe.defName).ForEach(delegate(Bill b)
        {
            b.suspended = true;
            b.deleted = true;
        });
        billStack.Bills.RemoveAll(b => b.recipe.defName == recipe.defName);
        materializeRecipeData.RemoveAll(r => r.defName == recipe.defName);
        allRecipes.RemoveAll(r => r.defName == recipe.defName);
    }

    public Bill MakeNewBill(RecipeDef recipe)
    {
        return new Bill_ProductionNotifyComplete(recipe);
    }

    private static void RegisterToEnergyRecipes()
    {
        if (energyThingDef == null)
        {
            energyThingDef = ThingDef.Named("NR_AutoMachineTool_MaterialEnergy");
        }

        if (toEnergyRecipes == null)
        {
            var toEnergyDefs = new HashSet<ThingDef>(from t in DefDatabase<ThingDef>.AllDefs
                where t.category == ThingCategory.Item
                where Ops.GetEnergyAmount(t) > 0.1f
                select t);
            toEnergyRecipes = new List<RecipeDef>
            {
                CreateToEnergyRecipeDef("NR_AutoMachineTool.ToEnergy10",
                    "NR_AutoMachineTool.MaterialMachine.RecipeToEnergyLabel".Translate(10),
                    "NR_AutoMachineTool.MaterialMachine.RecipeToEnergyJobName".Translate(10), 300f, 10, toEnergyDefs),
                CreateToEnergyRecipeDef("NR_AutoMachineTool.ToEnergy100",
                    "NR_AutoMachineTool.MaterialMachine.RecipeToEnergyLabel".Translate(100),
                    "NR_AutoMachineTool.MaterialMachine.RecipeToEnergyJobName".Translate(100), 1000f, 100, toEnergyDefs)
            };
            toEnergyRecipes.ForEach(delegate(RecipeDef r)
            {
                r.ingredientValueGetterClass = typeof(IngredientValueGetter_Energy);
                r.ingredientValueGetterInt = null;
            });
            DefDatabase<RecipeDef>.Add(toEnergyRecipes);
        }

        if (scanMaterialRecipe != null)
        {
            return;
        }

        var toEnergyDefs2 = new HashSet<ThingDef>(from t in DefDatabase<ThingDef>.AllDefs
            where t.category == ThingCategory.Item
            where Ops.GetEnergyAmount(t) > 0.1f
            select t);
        scanMaterialRecipe = CreateScanMaterialRecipeDef("NR_AutoMachineTool.ScanMaterial",
            "NR_AutoMachineTool.MaterialMachine.RecipeScanMaterialLabel".Translate(),
            "NR_AutoMachineTool.MaterialMachine.RecipeScanMaterialJobName".Translate(), 5000f, toEnergyDefs2);
        DefDatabase<RecipeDef>.Add(scanMaterialRecipe);
    }

    public override void ExposeData()
    {
        RegisterToEnergyRecipes();
        Scribe_Collections.Look(ref materializeRecipeData, "materializeRecipeData", LookMode.Deep);
        if (materializeRecipeData == null)
        {
            materializeRecipeData = new List<MaterializeRecipeDefData>();
        }

        materializeRecipeData.ForEach(delegate(MaterializeRecipeDefData d) { d.Register(Loss); });
        base.ExposeData();
    }

    public override void SpawnSetup(Map map, bool respawningAfterLoad)
    {
        base.SpawnSetup(map, respawningAfterLoad);
        RegisterToEnergyRecipes();
        allRecipes.Add(scanMaterialRecipe);
        allRecipes.AddRange(toEnergyRecipes);
        allRecipes.AddRange(materializeRecipeData.Select(r => r.GetRecipe()));
    }

    private static RecipeDef CreateRecipeDef(string defName, string label, string jobString, float workAmount)
    {
        return new RecipeDef
        {
            defName = defName,
            label = label,
            jobString = jobString,
            workAmount = workAmount,
            workSpeedStat = StatDefOf.WorkToMake,
            efficiencyStat = StatDefOf.WorkToMake,
            workSkill = SkillDefOf.Crafting,
            requiredGiverWorkType = WorkTypeDefOf.Crafting,
            workSkillLearnFactor = 0f
        };
    }

    private static RecipeDef CreateScanMaterialRecipeDef(string defName, string label, string jobString,
        float workAmount, HashSet<ThingDef> toEnergyDefs)
    {
        var r = CreateRecipeDef(defName, label, jobString, workAmount);
        var c = new IngredientCount();
        r.ingredients.Add(c);
        c.SetBaseCount(1f);
        c.filter = new ThingFilter();
        toEnergyDefs.ForEach(delegate(ThingDef d)
        {
            if (d.defName == "NR_AutoMachineTool_MaterialEnergy")
            {
                return;
            }

            r.fixedIngredientFilter.SetAllow(d, true);
            c.filter.SetAllow(d, true);
        });
        c.filter.RecalculateDisplayRootCategory();
        r.defaultIngredientFilter = new ThingFilter();
        r.defaultIngredientFilter.SetDisallowAll();
        r.fixedIngredientFilter.RecalculateDisplayRootCategory();
        r.ResolveReferences();
        r.allowMixingIngredients = true;
        return r;
    }

    private static RecipeDef CreateToEnergyRecipeDef(string defName, string label, string jobString, float workAmount,
        int count, HashSet<ThingDef> toEnergyDefs)
    {
        var r = CreateRecipeDef(defName, label, jobString, workAmount);
        var c = new IngredientCount();
        r.ingredients.Add(c);
        c.SetBaseCount(count);
        c.filter = new ThingFilter();
        toEnergyDefs.ForEach(delegate(ThingDef d)
        {
            if (d.defName == "NR_AutoMachineTool_MaterialEnergy")
            {
                return;
            }

            r.fixedIngredientFilter.SetAllow(d, true);
            c.filter.SetAllow(d, true);
        });
        c.filter.RecalculateDisplayRootCategory();
        r.defaultIngredientFilter = new ThingFilter();
        r.defaultIngredientFilter.SetDisallowAll();
        r.fixedIngredientFilter.RecalculateDisplayRootCategory();
        r.ResolveReferences();
        r.products.Add(new ThingDefCount(energyThingDef, count));
        r.allowMixingIngredients = true;
        return r;
    }

    private static RecipeDef CreateMaterializeRecipeDef(string defName, string label, string jobString,
        float workAmount, int energyCount, ThingDef product, int productCount, ThingDef stuff)
    {
        var recipeDef = CreateRecipeDef(defName, label, jobString, workAmount);
        var ingredientCount = new IngredientCount();
        recipeDef.ingredients.Add(ingredientCount);
        ingredientCount.SetBaseCount(energyCount);
        ingredientCount.filter = new ThingFilter();
        recipeDef.defaultIngredientFilter = new ThingFilter();
        if (stuff != null)
        {
            var ingredientCount2 = new IngredientCount();
            recipeDef.ingredients.Add(ingredientCount2);
            ingredientCount2.filter.SetAllow(stuff, true);
            ingredientCount2.SetBaseCount(10f);
        }

        ingredientCount.filter.SetAllow(energyThingDef, true);
        recipeDef.defaultIngredientFilter.SetAllow(energyThingDef, true);
        ingredientCount.filter.RecalculateDisplayRootCategory();
        recipeDef.fixedIngredientFilter.RecalculateDisplayRootCategory();
        recipeDef.ResolveReferences();
        recipeDef.products.Add(new ThingDefCount(product, productCount));
        return recipeDef;
    }

    public class MaterializeRecipeDefData : IExposable
    {
        public const string MaterializeRecipeDefPrefix = "NR_AutoMachineTool.Materialize_";
        public string defName;

        private int prodCount;

        private ThingDef stuff;

        private ThingDef thing;

        private int workAmount;

        public MaterializeRecipeDefData()
        {
        }

        public MaterializeRecipeDefData(ThingDef thing, ThingDef stuff, int prodCount, int workAmount)
        {
            this.thing = thing;
            this.stuff = stuff;
            this.prodCount = prodCount;
            this.workAmount = workAmount;
            defName = GetDefName(thing, stuff, prodCount);
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref defName, "defName");
            Scribe_Values.Look(ref prodCount, "prodCount");
            Scribe_Values.Look(ref workAmount, "workAmount");
            Scribe_Defs.Look(ref thing, "thing");
            Scribe_Defs.Look(ref stuff, "stuff");
        }

        public RecipeDef GetRecipe()
        {
            return DefDatabase<RecipeDef>.GetNamed(defName);
        }

        public void Register(float loss)
        {
            if (DefDatabase<RecipeDef>.GetNamedSilentFail(defName) != null)
            {
                return;
            }

            var num = Mathf.CeilToInt(Ops.GetEnergyAmount(thing, stuff) * prodCount / loss);
            var taggedString = "NR_AutoMachineTool.MaterialMachine.RecipeMaterializeLabel".Translate(thing.label,
                stuff == null
                    ? ""
                    : (string)"NR_AutoMachineTool.MaterialMachine.RecipeMaterializeStuff".Translate(stuff.label),
                num, prodCount);
            var taggedString2 =
                "NR_AutoMachineTool.MaterialMachine.RecipeMaterializeJobName".Translate(thing.label);
            DefDatabase<RecipeDef>.Add(CreateMaterializeRecipeDef(defName, taggedString, taggedString2, workAmount,
                num, thing, prodCount, stuff));
        }

        public static string GetDefName(ThingDef thing, ThingDef stuff, int prodCount)
        {
            return "NR_AutoMachineTool.Materialize_" + thing.defName + (stuff == null ? "" : "_" + stuff.defName) +
                   "_" + prodCount;
        }
    }
}