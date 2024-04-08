using System;
using RimWorld;
using UnityEngine;

namespace NR_AutoMachineTool;

public class MoteProgressBar2 : MoteProgressBar
{
    public Func<float> progressGetter;


    public override void DrawAt(Vector3 drawLoc, bool flip = false)
    {
        if (progressGetter != null)
        {
            progress = Mathf.Clamp01(progressGetter());
        }

        base.DrawAt(drawLoc, flip);
    }
}