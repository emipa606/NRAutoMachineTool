using RimWorld;
using Verse;

namespace NR_AutoMachineTool;

public interface IRecipeProductWorker
{
    Map Map { get; }

    IntVec3 Position { get; }

    Room GetRoom(RegionType type);

    int GetSkillLevel(SkillDef def);
}