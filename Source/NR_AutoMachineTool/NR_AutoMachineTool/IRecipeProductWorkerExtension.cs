using RimWorld;

namespace NR_AutoMachineTool;

public static class IRecipeProductWorkerExtension
{
    public static float GetStatValue(this IRecipeProductWorker maker, StatDef stat, bool applyPostProcess = true)
    {
        return stat == StatDefOf.FoodPoisonChance ? 0.0005f : 1f;
    }
}