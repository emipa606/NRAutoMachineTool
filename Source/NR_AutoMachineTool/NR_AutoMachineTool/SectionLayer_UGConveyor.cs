using RimWorld;
using Verse;

namespace NR_AutoMachineTool;

public class SectionLayer_UGConveyor : SectionLayer_Things
{
    public SectionLayer_UGConveyor(Section section)
        : base(section)
    {
        requireAddToMapMesh = false;
        relevantChangeTypes = MapMeshFlag.Buildings;
    }

    public override void DrawLayer()
    {
        if (OverlayDrawHandler_UGConveyor.ShouldDraw)
        {
            base.DrawLayer();
        }
    }

    public override void TakePrintFrom(Thing t)
    {
        if ((t.Faction == null || t.Faction == Faction.OfPlayer) && Building_BeltConveyor.IsBeltConveyorDef(t.def) &&
            Building_BeltConveyor.IsUndergroundDef(t.def))
        {
            t.Graphic.Print(this, t, 0f);
        }
    }
}