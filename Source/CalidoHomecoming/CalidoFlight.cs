using System.Linq;
using RimWorld;
using Verse;

namespace CalidoHomecoming
{
    public sealed class GenStep_SkyPort : GenStep
    {
        public override int SeedPart => 209187;
        public override void Generate(Map map, GenStepParams parms)
        {
            var floor = DefDatabase<TerrainDef>.GetNamed("PavedTile");
            foreach (IntVec3 cell in map.AllCells) map.terrainGrid.SetTerrain(cell, floor);
            foreach (int side in new[] { -1, 1 })
            {
                IntVec3 center = map.Center + new IntVec3(side * 12, 0, 3);
                foreach (IntVec3 cell in CellRect.CenteredOn(center, 13, 13).EdgeCells)
                {
                    if (cell.z == center.z && cell.x == center.x - side * 6) continue;
                    GenSpawn.Spawn(ThingMaker.MakeThing(ThingDefOf.Wall, ThingDefOf.Steel), cell, map);
                }
                var localKind = DefDatabase<PawnKindDef>.GetNamedSilentFail("Milira_Colonist");
                var portFaction = Find.FactionManager.AllFactions.FirstOrDefault(f => f.def.defName == "Calido_OrbitalMilira");
                if (localKind != null && portFaction != null)
                    GenSpawn.Spawn(PawnGenerator.GeneratePawn(localKind, portFaction), center + IntVec3.North * 3, map);
                GenSpawn.Spawn(ThingMaker.MakeThing(ThingDefOf.Table2x2c, ThingDefOf.WoodLog), center, map);
            }
        }
    }

    internal static class CalidoFlight
    {
        public static void Open()
        {
            Map origin = Find.CurrentMap;
            var generator = DefDatabase<MapGeneratorDef>.GetNamed("Calido_SkyCourt");
            if (origin == null) return;
            bool returning = origin.Parent.MapGeneratorDef == generator;
            if (!returning && !origin.IsPlayerHome)
            {
                Messages.Message("航班從殖民地或卡利多登城港出發。", MessageTypeDefOf.RejectInput);
                return;
            }
            var options = origin.mapPawns.FreeColonistsSpawned.Where(p => !p.Downed && !p.InMentalState)
                .Select(p => new FloatMenuOption(p.LabelShortCap + (returning ? "・返回南渡" : "・搭乘登城班機"), () =>
                {
                    Map destination = returning ? Find.AnyPlayerHomeMap : Find.Maps.FirstOrDefault(m => m.Parent.MapGeneratorDef == generator);
                    if (destination == null && !returning)
                        destination = PocketMapUtility.GeneratePocketMap(new IntVec3(65, 1, 65), generator, null, origin);
                    if (destination == null || !p.Spawned || p.Map != origin) return;
                    IntVec3 landing = CellFinder.RandomClosewalkCellNear(destination.Center, destination, 5);
                    p.jobs.StopAll();
                    p.DeSpawn();
                    GenSpawn.Spawn(p, landing, destination);
                    if (!returning) Current.Game.GetComponent<CalidoStoryState>().NotifyLanding(p);
                    CameraJumper.TryJumpAndSelect(p);
                })).ToList();
            if (options.Count > 0) Find.WindowStack.Add(new FloatMenu(options));
        }
    }
}
