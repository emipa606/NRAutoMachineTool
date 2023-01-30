using System.Collections.Generic;
using Verse;

namespace NR_AutoMachineTool;

internal interface IBeltConbeyorLinkable
{
    Rot4 Rotation { get; }

    IntVec3 Position { get; }

    bool IsUnderground { get; }

    IEnumerable<Rot4> OutputRots { get; }

    bool IsStuck { get; }

    void Link(IBeltConbeyorLinkable linkable);

    void Unlink(IBeltConbeyorLinkable linkable);

    bool ReceivableNow(bool underground, Thing thing);

    bool ReceiveThing(bool underground, Thing thing);
}