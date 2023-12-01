using RimWorld;
using UnityEngine;
using Verse;

namespace NR_AutoMachineTool;

public abstract class Graphic_Linked2 : Graphic
{
    public Graphic subGraphic;

    protected Material[] subMats = new Material[16];

    public Graphic_Linked2()
    {
        subGraphic = new Graphic_Single();
    }

    public override Material MatSingle => subMats[0];

    public override Material MatSingleFor(Thing thing)
    {
        return LinkedDrawMatFrom(thing, thing.Position);
    }

    public override void Init(GraphicRequest req)
    {
        data = req.graphicData;
        path = req.path;
        color = req.color;
        colorTwo = req.colorTwo;
        drawSize = req.drawSize;
        subGraphic.Init(req);
        CreateSubMats();
    }

    public void CreateSubMats()
    {
        var mainTextureScale = new Vector2(0.25f, 0.25f);
        for (var i = 0; i < 16; i++)
        {
            var x = i % 4f * 0.25f;
            // ReSharper disable once PossibleLossOfFraction
            var y = i / 4 * 0.25f;
            var mainTextureOffset = new Vector2(x, y);
            var material = new Material(subGraphic.MatSingle)
            {
                name = subGraphic.MatSingle.name + "_ASM" + i,
                mainTextureScale = mainTextureScale,
                mainTextureOffset = mainTextureOffset
            };
            subMats[i] = material;
        }
    }

    protected Material LinkedDrawMatFrom(Thing parent, IntVec3 cell)
    {
        var num = 0;
        var num2 = 1;
        for (var i = 0; i < 4; i++)
        {
            var c = cell + GenAdj.CardinalDirections[i];
            if (ShouldLinkWith(c, parent))
            {
                num += num2;
            }

            num2 *= 2;
        }

        var linkSet = (LinkDirections)num;
        return LinkedMaterial(parent, linkSet);
    }

    public virtual Material LinkedMaterial(Thing parent, LinkDirections linkSet)
    {
        return subMats[(uint)linkSet];
    }

    public abstract bool ShouldLinkWith(IntVec3 c, Thing parent);

    public override void Print(SectionLayer layer, Thing thing, float extraRotation)
    {
        var mat = LinkedDrawMatFrom(thing, thing.Position);
        Printer_Plane.PrintPlane(layer, thing.TrueCenter(), drawSize, mat);
    }

    public override void DrawWorker(Vector3 loc, Rot4 rot, ThingDef thingDef, Thing thing, float extraRotation)
    {
        GraphicDatabase
            .Get<Graphic_Single>(thingDef.uiIconPath, ShaderTypeDefOf.EdgeDetect.Shader, thingDef.graphicData.drawSize,
                color, colorTwo).DrawWorker(loc, rot, thingDef, thing, extraRotation);
    }
}

public abstract class Graphic_Linked2<T> : Graphic_Linked2 where T : Graphic_Linked2, new()
{
    public override Graphic GetColoredVersion(Shader newShader, Color newColor, Color newColorTwo)
    {
        var val = new T
        {
            subGraphic = subGraphic.GetColoredVersion(newShader, newColor, newColorTwo),
            data = data
        };
        val.CreateSubMats();
        return val;
    }
}