using UnityEngine;
using Verse;

namespace NR_AutoMachineTool;

[StaticConstructorOnStartup]
public class MoteLightning : MoteDualAttached
{
    private Material[] LightningMaterials;

    public override void SpawnSetup(Map map, bool respawningAfterLoad)
    {
        base.SpawnSetup(map, respawningAfterLoad);
        LightningMaterials = new Material[4];
        for (var i = 0; i < LightningMaterials.Length; i++)
        {
            var x = i % 2f * 0.5f;
            var y = i / 2f * 0.5f;
            LightningMaterials[i] = new Material(Graphic.MatSingle)
            {
                shader = ShaderDatabase.Transparent,
                name = "Thunder_" + i,
                mainTextureScale = new Vector2(0.5f, 0.5f),
                mainTextureOffset = new Vector2(x, y)
            };
        }
    }


    public override void DrawAt(Vector3 drawLoc, bool flip = false)
    {
        base.DrawAt(drawLoc, flip);
        var material = LightningMaterials[Find.TickManager.TicksAbs / 4 % 4];
        var num = (3 - (Find.TickManager.TicksAbs % 4)) / 3f * 0.5f;
        if (!(material != null))
        {
            return;
        }

        var drawPos = DrawPos;
        drawPos.y += 0.01f;
        var num2 = Alpha - 0.5f + num;
        if (num2 <= 0f)
        {
            return;
        }

        var color = instanceColor;
        color.a *= num2;
        if (color != material.color)
        {
            material.color = color;
        }

        var toDirection = link1.LastDrawPos - link2.LastDrawPos;
        var matrix = default(Matrix4x4);
        var q = Quaternion.FromToRotation(Vector3.forward, toDirection);
        matrix.SetTRS(drawPos, q,
            new Vector3(Mathf.Clamp(toDirection.magnitude / 5f, 1f, 2f), 1f, toDirection.magnitude));
        Graphics.DrawMesh(MeshPool.plane10, matrix, material, 0);
    }
}