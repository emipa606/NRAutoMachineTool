using NR_AutoMachineTool.Utilities;
using Verse;

namespace NR_AutoMachineTool;

public class IngredientValueGetter_Energy : IngredientValueGetter
{
    public override float ValuePerUnitOf(ThingDef t)
    {
        return Ops.GetEnergyAmount(t);
    }

    public override string BillRequirementsDescription(RecipeDef r, IngredientCount ing)
    {
        return "NR_AutoMachineTool.MaterialMachine.RecipeEnergyDescription".Translate(ing.GetBaseCount());
    }

    public override string ExtraDescriptionLine(RecipeDef r)
    {
        return null;
    }
}