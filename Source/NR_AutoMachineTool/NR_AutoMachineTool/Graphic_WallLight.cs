using System.Linq;
using UnityEngine;
using Verse;

namespace NR_AutoMachineTool;

public class Graphic_WallLight : Graphic_Linked2<Graphic_WallLight>
{
    public override bool ShouldDrawRotated => data == null || data.drawRotated;

    public override bool ShouldLinkWith(IntVec3 c, Thing parent)
    {
        return c.InBounds(parent.Map) && IsWall(c, parent.Map);
    }

    private bool IsWall(IntVec3 pos, Map map)
    {
        return (from t in pos.GetThingList(map)
            where t.def.category == ThingCategory.Building
            where t.def.building != null
            where !t.def.building.isNaturalRock
            select t).All(t => t.def.passability != Traversability.Impassable);
    }

    public override void DrawWorker(Vector3 loc, Rot4 rot, ThingDef thingDef, Thing thing, float extraRotation)
    {
        var num = 0;
        if (thingDef.PlaceWorkers.All(p => p.AllowsPlacing(thingDef, loc.ToIntVec3(), rot, Find.CurrentMap).Accepted))
        {
            var num2 = 1;
            for (var i = 0; i < 4; i++)
            {
                var pos = loc.ToIntVec3() + GenAdj.CardinalDirections[i];
                if (IsWall(pos, Find.CurrentMap))
                {
                    num += num2;
                }

                num2 *= 2;
            }
        }

        var linkDirections = (LinkDirections)num;
        var material = subMats[(uint)linkDirections];
        material.shader = ShaderDatabase.Transparent;
        var mesh = MeshAt(rot);
        var rotation = QuatFromRot(rot);
        if (extraRotation != 0f)
        {
            rotation *= Quaternion.Euler(Vector3.up * extraRotation);
        }

        Graphics.DrawMesh(mesh, loc, rotation, material, 0);
        ShadowGraphic?.DrawWorker(loc, rot, thingDef, thing, extraRotation);
    }
}