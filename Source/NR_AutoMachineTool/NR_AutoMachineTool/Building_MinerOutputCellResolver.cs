using NR_AutoMachineTool.Utilities;
using Verse;

namespace NR_AutoMachineTool;

public class Building_MinerOutputCellResolver : OutputCellResolver
{
    public override Option<IntVec3> OutputCell(IntVec3 cell, Map map, Rot4 rot)
    {
        return from b in cell.GetThingList(map).SelectMany(b => Ops.Option(b as Building_Miner)).FirstOption()
            select b.OutputCell();
    }
}