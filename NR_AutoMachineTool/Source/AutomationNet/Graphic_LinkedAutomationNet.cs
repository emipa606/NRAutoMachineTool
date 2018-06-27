﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using Verse;
using RimWorld;

using NR_AutoMachineTool.Utilities;
using static NR_AutoMachineTool.Utilities.Ops;

namespace NR_AutoMachineTool
{
    public class Graphic_LinkedAutomationNet : Graphic_Linked
    {
        public Graphic_LinkedAutomationNet() : base()
        {
        }

        public override void Init(GraphicRequest req)
        {
            this.subGraphic = new Graphic_Single();
            this.subGraphic.Init(req);
            this.path = req.path;
        }

        public override bool ShouldLinkWith(IntVec3 c, Thing parent)
        {
            var parentCheck = parent.TryGetComp<CompAutomation>() != null ||
                Option(parent as Blueprint).SelectMany(b => Option(b.def.entityDefToBuild as ThingDef)).Select(d => d.GetCompProperties<CompProperties_Automation>() != null).GetOrDefault(false);

            var cellCheck = 
                c.GetThingList(parent.Map)
                    .SelectMany(t => Option(t as Building))
                    .Any(b => b.TryGetComp<CompAutomation>() != null) ||
                c.GetThingList(parent.Map)
                    .SelectMany(t => Option(t as Blueprint))
                    .SelectMany(b => Option(b.def.entityDefToBuild as ThingDef))
                    .Any(d => d.GetCompProperties<CompProperties_Automation>() != null);

            return c.InBounds(parent.Map) && (parentCheck && cellCheck);
        }

        public override Graphic GetColoredVersion(Shader newShader, Color newColor, Color newColorTwo)
        {
            Graphic_LinkedAutomationNet g = new Graphic_LinkedAutomationNet();
            g.subGraphic = this.subGraphic.GetColoredVersion(newShader, newColor, newColorTwo);
            g.data = this.data;
            return g;
        }

        public override void DrawWorker(Vector3 loc, Rot4 rot, ThingDef thingDef, Thing thing, float extraRotation)
        {
            Material material = this.MatAt(rot, thing);
            GraphicDatabase.Get<Graphic_Single>(thingDef.uiIconPath, ShaderTypeDefOf.EdgeDetect.Shader, thingDef.graphicData.drawSize, material.color, material.GetColorTwo())
                .DrawWorker(loc, rot, thingDef, thing, extraRotation);
        }
    }
}
