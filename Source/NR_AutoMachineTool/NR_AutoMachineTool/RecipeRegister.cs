using System.Collections.Generic;
using System.Linq;
using NR_AutoMachineTool.Utilities;
using RimWorld;
using UnityEngine;
using Verse;

namespace NR_AutoMachineTool;

[StaticConstructorOnStartup]
public static class RecipeRegister
{
    static RecipeRegister()
    {
        var list = (from d in DefDatabase<ThingDef>.AllDefs
            where d.mineable && d.building is { mineableThing: { }, mineableYield: > 0 }
            where d.building.isResourceRock || d.building.isNaturalRock
            select new ThingDefCountClass(d.building.mineableThing, d.building.mineableYield)).ToList();
        var mineablesSet = Ops.ToHashSet(list.Select(d => d.thingDef));
        list.AddRange(from d in DefDatabase<ThingDef>.AllDefs
            where d.deepCommonality > 0f && d.deepCountPerPortion > 0
            where !mineablesSet.Contains(d)
            select new ThingDefCountClass(d, d.deepCountPerPortion));
        var list2 = list.Select(CreateMiningRecipe).ToList();
        DefDatabase<RecipeDef>.Add(list2);
        DefDatabase<ThingDef>.GetNamed("Building_NR_AutoMachineTool_Miner").recipes = list2;
    }

    private static RecipeDef CreateMiningRecipe(ThingDefCountClass defCount)
    {
        var result = new RecipeDef
        {
            defName = "Recipe_NR_AutoMachineTool_Mine_" + defCount.thingDef.defName,
            label = "NR_AutoMachineTool.AutoMiner.MineOre".Translate(defCount.thingDef.label),
            jobString = "NR_AutoMachineTool.AutoMiner.MineOre".Translate(defCount.thingDef.label),
            workAmount = Mathf.Max(10000f,
                StatDefOf.MarketValue.Worker.GetValue(StatRequest.For(defCount.thingDef, null)) * defCount.count *
                1000f),
            workSpeedStat = StatDefOf.WorkToMake,
            efficiencyStat = StatDefOf.WorkToMake,
            workSkill = SkillDefOf.Mining,
            workSkillLearnFactor = 0f,
            products = new List<ThingDefCountClass>().Append(defCount),
            defaultIngredientFilter = new ThingFilter(),
            effectWorking = EffecterDefOf.Drill
        };
        if (!defCount.thingDef.statBases.StatListContains(StatDefOf.MarketValue) && defCount.count == 1)
        {
            defCount.thingDef.BaseMarketValue = 0f;
        }

        return result;
    }
}