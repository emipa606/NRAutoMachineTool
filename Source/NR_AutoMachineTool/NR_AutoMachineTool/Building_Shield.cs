using System.Collections.Generic;
using System.Linq;
using NR_AutoMachineTool.Utilities;
using RimWorld;
using UnityEngine;
using Verse;

namespace NR_AutoMachineTool;

public class Building_Shield : Building_BaseRange<Thing>
{
    private static readonly HashSet<Thing> workingSet = new HashSet<Thing>();
    private static float angle;

    public Building_Shield()
    {
        startCheckIntervalTicks = 5;
        readyOnStart = true;
        targetEnumrationCount = 0;
    }

    protected override float SpeedFactor => 1f;

    public override int MinPowerForSpeed => 10000;

    public override int MaxPowerForSpeed => 10000;

    public override bool SpeedSetting => false;

    protected override bool WorkInterruption(Thing working)
    {
        return !working.Spawned;
    }

    public override void Draw()
    {
        base.Draw();
        if (!IsActive())
        {
            return;
        }


        if (!Find.TickManager.Paused)
        {
            angle++;
            if (angle > 360f)
            {
                angle = 0;
            }
        }

        var range = GetRange() * 2;
        var vector = DrawPos;
        vector.y = AltitudeLayer.MoteOverhead.AltitudeFor();
        var s = new Vector3(range, 1f, range);
        var matrix = default(Matrix4x4);
        matrix.SetTRS(vector, Quaternion.AngleAxis(angle, Vector3.up), s);
        Graphics.DrawMesh(MeshPool.plane10, matrix, CompShield.BubbleMat, 0);
    }

    protected override bool TryStartWorking(out Thing target, out float workAmount)
    {
        var cells = GetAllTargetCells();
        (from f in (from Skyfaller f in Map.listerThings.ThingsOfDef(ThingDefOf.DropPodIncoming)
                    where cells.Contains(f.Position)
                    where f.innerContainer.SelectMany(t => Ops.Option(t as IActiveDropPod))
                        .SelectMany(d => d.Contents.innerContainer).Any(i => Faction.OfPlayer.HostileTo(i.Faction))
                    select f).Concat(Map.listerThings.ThingsOfDef(ThingDefOf.CrashedShipPartIncoming).Cast<Skyfaller>())
                .Concat(Map.listerThings.ThingsOfDef(ThingDefOf.ShipChunkIncoming).Cast<Skyfaller>())
                .Concat(Map.listerThings.ThingsOfDef(ThingDefOf.MeteoriteIncoming).Cast<Skyfaller>())
            where cells.Contains(f.Position)
            where !workingSet.Contains(f)
            select f).Peek(delegate(Skyfaller f) { workingSet.Add(f); }).ForEach(delegate(Skyfaller f)
        {
            MapManager.AfterAction(f.ticksToImpact - 17, delegate { DestroySkyfaller(f); });
        });
        (from f in MapManager.ThingsList.ForAssignableFrom<Projectile>()
            where cells.Contains(f.Position)
            select f
            into p
            where p.launcher.Faction != Faction.OfPlayer
            select p).ToList().ForEach(DestroyProjectile);
        workAmount = 0f;
        target = null;
        return false;
    }

    protected void DestroySkyfaller(Skyfaller faller)
    {
        if (IsActive())
        {
            GenExplosion.DoExplosion(faller.DrawPos.ToIntVec3(), Map, 1f, DamageDefOf.Bomb, faller, 0);
            faller.Destroy();
        }

        workingSet.Remove(faller);
    }

    protected void DestroyProjectile(Projectile proj)
    {
        if (!IsActive())
        {
            return;
        }

        FleckMaker.ThrowLightningGlow(proj.DrawPos, Map, 1f);
        proj.Destroy();
    }

    protected override bool FinishWorking(Thing working, out List<Thing> products)
    {
        products = new List<Thing>();
        return false;
    }
}