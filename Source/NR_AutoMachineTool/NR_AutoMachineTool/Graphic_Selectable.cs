using System.Collections.Generic;
using System.Linq;
using NR_AutoMachineTool.Utilities;
using UnityEngine;
using Verse;

namespace NR_AutoMachineTool;

internal class Graphic_Selectable : Graphic_Collection
{
    private readonly Dictionary<string, Graphic> pathDic = new Dictionary<string, Graphic>();

    public override Material MatSingle => subGraphics[0].MatSingle;

    public override bool ShouldDrawRotated => true;

    public Graphic Get(string path)
    {
        if (path == null)
        {
            Ops.Option(subGraphics[0].data).ForEach(delegate(GraphicData d) { d.drawRotated = true; });
            return subGraphics[0];
        }

        if (pathDic.TryGetValue(path, out var value))
        {
            return value;
        }

        pathDic[path] = subGraphics.First(x => x.path == path);
        Ops.Option(pathDic[path].data).ForEach(delegate(GraphicData d) { d.drawRotated = true; });

        return pathDic[path];
    }

    public override void Init(GraphicRequest req)
    {
        base.Init(req);
        subGraphics.ForEach(delegate(Graphic g)
        {
            Ops.Option(g.data).ForEach(delegate(GraphicData d) { d.drawRotated = true; });
        });
    }

    public override Graphic GetColoredVersion(Shader newShader, Color newColor, Color newColorTwo)
    {
        return subGraphics[0].GetColoredVersion(newShader, newColor, newColorTwo);
    }
}