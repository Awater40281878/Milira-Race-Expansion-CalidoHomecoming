using System.Linq;
using RimWorld;
using Verse;

namespace CalidoHomecoming
{
    public static class CalidoStartingGuide
    {
        public const float BuildingWealthTarget = 300f;

        public static bool[] Progress(Map map)
        {
            if (map == null) return new bool[4];
            bool shelter = map.mapPawns.FreeColonistsSpawned.Any(p =>
            {
                Building_Bed bed = p.ownership?.OwnedBed;
                Room room = bed?.GetRoom();
                return room != null && room.ProperRoom && !room.PsychologicallyOutdoors && room.OpenRoofCount == 0;
            }) || map.listerBuildings.AllBuildingsColonistOfClass<Building_Bed>().Any(b =>
            {
                Room room = b.GetRoom();
                return room != null && room.ProperRoom && !room.PsychologicallyOutdoors && room.OpenRoofCount == 0;
            });

            bool stockpile = map.zoneManager.AllZones.OfType<Zone_Stockpile>()
                .Any(z => z.Cells.Any(c =>
                {
                    Room room = c.GetRoom(map);
                    return room != null && room.ProperRoom && !room.PsychologicallyOutdoors && room.OpenRoofCount == 0;
                }));
            bool cooking = map.listerBuildings.AllBuildingsColonistOfClass<Building>().Any(b =>
                b.def.defName == "Campfire" || b.def.defName == "FueledStove" || b.def.defName == "ElectricStove");
            return new[] { shelter, stockpile, cooking, map.wealthWatcher.WealthBuildings >= BuildingWealthTarget };
        }
    }
}
