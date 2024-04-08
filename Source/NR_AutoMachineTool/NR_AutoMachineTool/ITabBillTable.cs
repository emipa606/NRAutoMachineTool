using System.Collections.Generic;
using RimWorld;
using Verse;

namespace NR_AutoMachineTool;

public interface ITabBillTable
{
    ThingDef def { get; }

    BillStack billStack { get; }

    Map Map { get; }

    IntVec3 Position { get; }

    IEnumerable<RecipeDef> AllRecipes { get; }

    Bill MakeNewBill(RecipeDef recipe);

    bool IsRemovable(RecipeDef recipe);

    void RemoveRecipe(RecipeDef recipe);
}