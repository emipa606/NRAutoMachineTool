using System.Collections.Generic;
using Verse;

namespace NR_AutoMachineTool;

public interface IRange
{
    IntVec3 Position { get; }

    Rot4 Rotation { get; }

    int GetRange();

    IEnumerable<IntVec3> GetAllTargetCells();
}