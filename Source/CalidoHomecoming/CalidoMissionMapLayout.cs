using RimWorld;
using UnityEngine;
using Verse;

namespace CalidoHomecoming
{
    // Compact, deterministic map set pieces. They deliberately use only Core
    // buildings so a mission stays playable even when optional race mods change.
    internal static class CalidoMissionMapLayout
    {
        public static IntVec3 Build(CalidoMissionSite site, Map map)
        {
            IntVec3 anchor = FindAnchor(map);
            int beat = site.campaignBeat;
            if (beat == -2 || beat <= 3)
                BuildSlaveCompound(map, anchor, beat == 1);
            else if (beat == 15)
                BuildMechRelay(map, anchor);
            else if (beat >= 16)
                BuildScriptorium(map, anchor);
            else
                BuildInspectionPost(map, anchor);
            return anchor;
        }

        private static IntVec3 FindAnchor(Map map)
        {
            for (int i = 0; i < 80; i++)
            {
                IntVec3 cell = CellFinder.RandomClosewalkCellNear(map.Center, map, 35);
                if (cell.InBounds(map) && cell.Walkable(map)) return cell;
            }
            return map.Center;
        }

        private static void BuildSlaveCompound(Map map, IntVec3 c, bool market)
        {
            BuildEnclosure(map, c, 10, 7, 3);
            SpawnResource(map, c + new IntVec3(-5, 0, -3), ThingDefOf.Steel, 45);
            SpawnResource(map, c + new IntVec3(5, 0, 3), ThingDefOf.ComponentIndustrial, 2);
            if (market)
            {
                BuildEnclosure(map, c + new IntVec3(0, 0, 11), 8, 4, 2);
                SpawnResource(map, c + new IntVec3(-2, 0, 11), ThingDefOf.Silver, 110);
                SpawnResource(map, c + new IntVec3(2, 0, 11), ThingDefOf.Gold, 8);
            }
        }

        private static void BuildInspectionPost(Map map, IntVec3 c)
        {
            BuildEnclosure(map, c, 9, 6, 3);
            BuildEnclosure(map, c + new IntVec3(0, 0, 10), 5, 3, 1);
            SpawnSandbagLine(map, c + new IntVec3(-8, 0, -8), 7);
            SpawnResource(map, c + new IntVec3(0, 0, 9), ThingDefOf.ComponentIndustrial, 3);
        }

        private static void BuildMechRelay(Map map, IntVec3 c)
        {
            BuildEnclosure(map, c, 12, 12, 4);
            SpawnSandbagLine(map, c + new IntVec3(-10, 0, -11), 9);
            SpawnSandbagLine(map, c + new IntVec3(2, 0, -11), 9);
            SpawnResource(map, c + new IntVec3(-4, 0, 0), ThingDefOf.ComponentIndustrial, 6);
            SpawnResource(map, c + new IntVec3(4, 0, 0), ThingDefOf.Steel, 120);
        }

        private static void BuildScriptorium(Map map, IntVec3 c)
        {
            BuildEnclosure(map, c, 11, 8, 2);
            BuildEnclosure(map, c + new IntVec3(0, 0, 12), 6, 4, 1);
            SpawnResource(map, c + new IntVec3(-4, 0, 1), ThingDefOf.WoodLog, 70);
            SpawnResource(map, c + new IntVec3(4, 0, 1), ThingDefOf.MedicineIndustrial, 5);
        }

        private static void BuildEnclosure(Map map, IntVec3 center, int halfWidth, int halfHeight, int gateHalfWidth)
        {
            for (int x = -halfWidth; x <= halfWidth; x++)
            {
                SpawnWall(map, center + new IntVec3(x, 0, -halfHeight));
                SpawnWall(map, center + new IntVec3(x, 0, halfHeight));
            }
            for (int z = -halfHeight + 1; z < halfHeight; z++)
            {
                SpawnWall(map, center + new IntVec3(-halfWidth, 0, z));
                if (Mathf.Abs(z) > gateHalfWidth) SpawnWall(map, center + new IntVec3(halfWidth, 0, z));
            }
        }

        private static void SpawnWall(Map map, IntVec3 cell)
        {
            if (!cell.InBounds(map) || !cell.Walkable(map) || cell.GetEdifice(map) != null) return;
            Thing wall = ThingMaker.MakeThing(ThingDefOf.Wall, ThingDefOf.Steel);
            GenSpawn.Spawn(wall, cell, map, WipeMode.Vanish);
        }

        private static void SpawnSandbagLine(Map map, IntVec3 start, int length)
        {
            for (int i = 0; i < length; i++)
            {
                IntVec3 cell = start + new IntVec3(i, 0, 0);
                if (!cell.InBounds(map) || !cell.Walkable(map) || cell.GetEdifice(map) != null) continue;
                GenSpawn.Spawn(ThingMaker.MakeThing(ThingDefOf.Sandbags), cell, map, WipeMode.Vanish);
            }
        }

        private static void SpawnResource(Map map, IntVec3 cell, ThingDef def, int count)
        {
            if (def == null || !cell.InBounds(map) || !cell.Walkable(map)) return;
            Thing thing = ThingMaker.MakeThing(def);
            thing.stackCount = Mathf.Min(count, def.stackLimit);
            GenSpawn.Spawn(thing, cell, map, WipeMode.Vanish);
        }
    }
}
