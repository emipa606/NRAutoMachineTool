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
    public class Graphic_LinkedConveyor : Graphic_Link2<Graphic_LinkedConveyor>
    {
        public Graphic_LinkedConveyor() : base()
        {
        }

        private Material arrow00 = MaterialPool.MatFrom("NR_AutoMachineTool/Buildings/BeltConveyor_arrow00");
        private Material arrow01 = MaterialPool.MatFrom("NR_AutoMachineTool/Buildings/BeltConveyor_arrow01");

        public override bool ShouldLinkWith(IntVec3 c, Thing parent)
        {
            if (!c.InBounds(parent.Map))
            {
                return false;
            }

            if(parent.Position + parent.Rotation.FacingCell == c)
            {
                return true;
            }

            var blueprint = parent as Blueprint != null;
            var def = blueprint ? (ThingDef)parent.def.entityDefToBuild : parent.def;

            var cellThing = blueprint ?
                c.GetThingList(parent.Map)
                    .SelectMany(t => Option(t as Blueprint))
                    .Where(b => b.def.entityDefToBuild as ThingDef != null)
                    .Select(b => new { Thing = (Thing)b, Def = (ThingDef)b.def.entityDefToBuild })
                    .Where(b => Building_BeltConveyor.IsBeltConveyorDef(b.Def) || Building_BeltConveyorUGConnecter.IsConveyorUGConnecterDef(b.Def))
                    .FirstOption().GetOrDefault(null) :
                c.GetThingList(parent.Map)
                    .SelectMany(t => Option(t as Building))
                    .Select(b => new { Thing = (Thing)b, Def = b.def})
                    .Where(b => Building_BeltConveyor.IsBeltConveyorDef(b.Def) || Building_BeltConveyorUGConnecter.IsConveyorUGConnecterDef(b.Def))
                    .FirstOption().GetOrDefault(null);

            if(cellThing == null)
            {
                return false;
            }
            return Building_BeltConveyor.CanLink(parent, cellThing.Thing, def, cellThing.Def);
        }

        public override bool ShouldDrawRotated
        {
            get
            {
                return this.data == null || this.data.drawRotated;
            }
        }

        public override void Print(SectionLayer layer, Thing thing, float extraRotation)
        {
            if (thing is Blueprint)
            {
                base.Print(layer, thing, 0);
                Printer_Plane.PrintPlane(layer, thing.TrueCenter() + new Vector3(0, 0.1f, 0), this.drawSize, this.arrow00, thing.Rotation.AsAngle);
            }
            else
            {
                var conveyor = thing as IBeltConbeyorLinkable;
                if (!Building_BeltConveyorUGConnecter.IsConveyorUGConnecterDef(thing.def) && conveyor != null && conveyor.IsUnderground && !(layer is SectionLayer_UGConveyor))
                {
                    return;
                }

                base.Print(layer, thing, 0);
                Printer_Plane.PrintPlane(layer, thing.TrueCenter() + new Vector3(0, 0.1f, 0), this.drawSize, this.arrow00, thing.Rotation.AsAngle);
                if (conveyor != null)
                {
                    conveyor.OutputRots.Where(x => x != thing.Rotation)
                        .ForEach(r => Printer_Plane.PrintPlane(layer, thing.TrueCenter(), this.drawSize, this.arrow01, r.AsAngle));
                }
            }
        }
    }
}
