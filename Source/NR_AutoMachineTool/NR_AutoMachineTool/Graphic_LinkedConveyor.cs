using System.Linq;
using NR_AutoMachineTool.Utilities;
using RimWorld;
using UnityEngine;
using Verse;

namespace NR_AutoMachineTool;

public class Graphic_LinkedConveyor : Graphic_Linked2<Graphic_LinkedConveyor>
{
    private readonly Material arrow00 = MaterialPool.MatFrom("NR_AutoMachineTool/Buildings/BeltConveyor_arrow00");

    private readonly Material arrow01 = MaterialPool.MatFrom("NR_AutoMachineTool/Buildings/BeltConveyor_arrow01");

    public override bool ShouldDrawRotated => data == null || data.drawRotated;

    public override bool ShouldLinkWith(IntVec3 c, Thing parent)
    {
        if (!c.InBounds(parent.Map))
        {
            return false;
        }

        if (parent.Position + parent.Rotation.FacingCell == c)
        {
            return true;
        }

        var num = parent is Blueprint;
        var thisDef = num ? (ThingDef)parent.def.entityDefToBuild : parent.def;
        var anon = num
            ? (from b in c.GetThingList(parent.Map).SelectMany(t => Ops.Option(t as Blueprint))
                where b.def.entityDefToBuild is ThingDef
                select new
                {
                    Thing = (Thing)b,
                    Def = (ThingDef)b.def.entityDefToBuild
                }
                into b
                where Building_BeltConveyor.IsBeltConveyorDef(b.Def) ||
                      Building_BeltConveyorUGConnecter.IsConveyorUGConnecterDef(b.Def)
                select b).FirstOption().GetOrDefault(null)
            : (from b in c.GetThingList(parent.Map).SelectMany(t => Ops.Option(t as Building))
                select new
                {
                    Thing = (Thing)b,
                    Def = b.def
                }
                into b
                where Building_BeltConveyor.IsBeltConveyorDef(b.Def) ||
                      Building_BeltConveyorUGConnecter.IsConveyorUGConnecterDef(b.Def)
                select b).FirstOption().GetOrDefault(null);
        return anon != null && Building_BeltConveyor.CanLink(parent, anon.Thing, thisDef, anon.Def);
    }

    public override void Print(SectionLayer layer, Thing thing, float extraRotation)
    {
        if (thing is Blueprint)
        {
            base.Print(layer, thing, extraRotation);
            Printer_Plane.PrintPlane(layer, thing.TrueCenter() + new Vector3(0f, 0.1f, 0f), drawSize, arrow00,
                thing.Rotation.AsAngle);
            return;
        }

        var beltConbeyorLinkable = thing as IBeltConbeyorLinkable;
        if (!Building_BeltConveyorUGConnecter.IsConveyorUGConnecterDef(thing.def) &&
            beltConbeyorLinkable is { IsUnderground: true } && layer is not SectionLayer_UGConveyor)
        {
            return;
        }

        base.Print(layer, thing, extraRotation);
        Printer_Plane.PrintPlane(layer, thing.TrueCenter() + new Vector3(0f, 0.1f, 0f), drawSize, arrow00,
            thing.Rotation.AsAngle);
        beltConbeyorLinkable?.OutputRots.Where(x => x != thing.Rotation).ForEach(delegate(Rot4 r)
        {
            Printer_Plane.PrintPlane(layer, thing.TrueCenter(), drawSize, arrow01, r.AsAngle);
        });
    }
}