using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace NR_AutoMachineTool;

internal static class GenRecipe2
{
    public static IEnumerable<Thing> MakeRecipeProducts(RecipeDef recipeDef, IRecipeProductWorker worker,
        List<Thing> ingredients, Thing dominantIngredient, IBillGiver billGiver)
    {
        var result = MakeRecipeProductsInt(recipeDef, worker, ingredients, dominantIngredient, billGiver);
        LoadedModManager.GetMod<Mod_AutoMachineTool>().Hopm.ForEach(delegate(Mod_AutoMachineTool.HopmMod m)
        {
            m.Postfix_MakeRecipeProducts(ref result, recipeDef, 1f, ingredients);
        });
        return result;
    }

    private static IEnumerable<Thing> MakeRecipeProductsInt(RecipeDef recipeDef, IRecipeProductWorker worker,
        List<Thing> ingredients, Thing dominantIngredient, IBillGiver billGiver)
    {
        var efficiency = recipeDef.efficiencyStat != null ? worker.GetStatValue(recipeDef.efficiencyStat) : 1f;
        if (recipeDef.workTableEfficiencyStat != null && billGiver is Building_WorkTable building_WorkTable)
        {
            efficiency *= building_WorkTable.GetStatValue(recipeDef.workTableEfficiencyStat);
        }

        if (recipeDef.products != null)
        {
            // ReSharper disable once ForCanBeConvertedToForeach
            for (var j = 0; j < recipeDef.products.Count; j++)
            {
                var thingDefCountClass = recipeDef.products[j];
                var thing = ThingMaker.MakeThing(
                    stuff: !thingDefCountClass.thingDef.MadeFromStuff ? null : dominantIngredient?.def,
                    def: thingDefCountClass.thingDef);
                thing.stackCount = Mathf.CeilToInt(thingDefCountClass.count * efficiency);
                if (dominantIngredient != null)
                {
                    thing.SetColor(dominantIngredient.DrawColor, false);
                }

                var compIngredients = thing.TryGetComp<CompIngredients>();
                if (compIngredients != null)
                {
                    foreach (var ingredientThing in ingredients)
                    {
                        compIngredients.RegisterIngredient(ingredientThing.def);
                    }
                }

                var compFoodPoisonable = thing.TryGetComp<CompFoodPoisonable>();
                if (compFoodPoisonable != null)
                {
                    if (Rand.Chance(worker.GetRoom(RegionType.Set_Passable)?.GetStat(RoomStatDefOf.FoodPoisonChance) ??
                                    RoomStatDefOf.FoodPoisonChance.roomlessScore))
                    {
                        compFoodPoisonable.SetPoisoned(FoodPoisonCause.FilthyKitchen);
                    }
                    else if (Rand.Chance(worker.GetStatValue(StatDefOf.FoodPoisonChance)))
                    {
                        compFoodPoisonable.SetPoisoned(FoodPoisonCause.IncompetentCook);
                    }
                }

                yield return PostProcessProduct(thing, recipeDef, worker);
            }
        }

        if (recipeDef.specialProducts == null)
        {
            yield break;
        }

        // ReSharper disable once ForCanBeConvertedToForeach
        for (var j = 0; j < recipeDef.specialProducts.Count; j++)
        {
            // ReSharper disable once ForCanBeConvertedToForeach
            for (var k = 0; k < ingredients.Count; k++)
            {
                var thing2 = ingredients[k];
                switch (recipeDef.specialProducts[j])
                {
                    case SpecialProductType.Smelted:
                        foreach (var item in thing2.SmeltProducts(efficiency))
                        {
                            yield return PostProcessProduct(item, recipeDef, worker);
                        }

                        break;
                    case SpecialProductType.Butchery:
                        foreach (var item2 in ButcherProducts(thing2, efficiency, worker))
                        {
                            yield return PostProcessProduct(item2, recipeDef, worker);
                        }

                        break;
                }
            }
        }
    }

    private static Thing PostProcessProduct(Thing product, RecipeDef recipeDef, IRecipeProductWorker worker)
    {
        var compQuality = product.TryGetComp<CompQuality>();
        if (compQuality != null)
        {
            if (recipeDef.workSkill == null)
            {
                Log.Error(recipeDef + " needs workSkill because it creates a product with a quality.");
            }

            var q = QualityUtility.GenerateQualityCreatedByPawn(worker.GetSkillLevel(recipeDef.workSkill), false);
            compQuality.SetQuality(q, ArtGenerationContext.Colony);
        }

        if (product.TryGetComp<CompArt>() != null)
        {
            if (compQuality != null)
            {
                _ = compQuality.Quality;
            }

            _ = 4;
        }

        if (product.def.Minifiable)
        {
            product = product.MakeMinified();
        }

        return product;
    }

    private static IEnumerable<Thing> ButcherProducts(Thing thing, float efficiency, IRecipeProductWorker worker)
    {
        if (thing is Corpse corpse)
        {
            return ButcherProducts(corpse, efficiency, worker);
        }

        return thing.ButcherProducts(null, efficiency);
    }

    private static IEnumerable<Thing> ButcherProducts(Corpse corpse, float efficiency, IRecipeProductWorker worker)
    {
        foreach (var item in corpse.InnerPawn.ButcherProducts(null, efficiency))
        {
            yield return item;
        }

        if (corpse.InnerPawn.RaceProps.BloodDef != null)
        {
            FilthMaker.TryMakeFilth(worker.Position, worker.Map, corpse.InnerPawn.RaceProps.BloodDef,
                corpse.InnerPawn.LabelIndefinite());
        }
    }
}