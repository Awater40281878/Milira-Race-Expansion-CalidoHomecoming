using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.AI.Group;

namespace CalidoHomecoming
{
    public static class CalidoHeroEncounter
    {
        private static readonly string[] Places =
        {
            "南渡舊渡口", "霧谷醫療站", "北道帳房", "斷索獵徑", "舊橋工坊",
            "廢棄哨樓", "靜默航標站", "無名劍塚", "石門記錄所", "河灣種子庫",
            "白羽修繕室", "廢棄稅所", "高岩瞭望塔", "旅人灶間", "盾痕關口",
            "枯林靶場", "沉默機修棚", "暮影小徑", "殘頁藏書室", "黑羽守望台"
        };

        public static string PlaceName(int index) => Places[index];

        public static void Populate(CalidoMissionSite site, Map map)
        {
            CalidoHeroRegistry registry = Current.Game.GetComponent<CalidoHeroRegistry>();
            Pawn hero = registry?.EnsureHero(site.heroIndex);
            if (hero == null || hero.Dead || hero.Spawned) return;
            IntVec3 near = CalidoMissionMapLayout.Build(site, map);
            Faction guild = Find.FactionManager.AllFactions.FirstOrDefault(f => f.def.defName == "Calido_AdventurersGuild");
            if (Find.WorldPawns.Contains(hero)) Find.WorldPawns.RemovePawn(hero);
            hero.SetFaction(guild);
            GenSpawn.Spawn(hero, CellFinder.RandomClosewalkCellNear(near, map, 4), map);
            if (guild != null) LordMaker.MakeNewLord(guild, new LordJob_DefendPoint(near), map, new[] { hero });
            GenPlace.TryPlaceThing(ThingMaker.MakeThing(DefDatabase<ThingDef>.GetNamed("Calido_HeroMeetingMarker")),
                near, map, ThingPlaceMode.Near);
        }
    }

    public sealed class CalidoHeroMeetingMarker : Building
    {
        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo item in base.GetGizmos()) yield return item;
            if (!(Map.Parent is CalidoMissionSite site) || site.heroIndex < 1) yield break;
            CalidoHeroRegistry registry = Current.Game.GetComponent<CalidoHeroRegistry>();
            Pawn hero = registry?.FindHero(site.heroIndex);
            if (hero == null || hero.Dead || registry.HasJoined(site.heroIndex)) yield break;
            var command = new Command_Action
            {
                defaultLabel = "與旅人交談",
                defaultDesc = "讓一名清醒的殖民者走到行囊旁，再與旅人交談。"
            };
            if (!Map.mapPawns.FreeColonistsSpawned.Any(p => !p.Downed && p.Position.DistanceTo(Position) <= 3f))
                command.Disable("需要一名殖民者靠近行囊");
            if (!hero.Spawned || hero.Map != Map || hero.Downed)
                command.Disable("旅人需要保持清醒並留在地圖上");
            command.action = () =>
            {
                CampaignBeat beat = CalidoCampaignContent.Beats[site.heroIndex - 1];
                HeroPersonalLine line = CalidoHeroPersonalContent.Get(beat.HeroName);
                Find.WindowStack.Add(new Dialog_ChapterOneEncounter("旅途相遇", beat.HeroName + "・" + beat.HeroTitle,
                    "「" + line.Request + "」\n\n她收起行囊，望向南渡隊伍。\n\n「我還有事要做。你願意讓我同行嗎？」",
                    new Dialog_ChapterOneEncounter.Choice("邀請同行", "她將加入隊伍並保留個人紀錄。",
                        () => Current.Game.GetComponent<CalidoStoryState>().RecruitHeroAtSite(beat, hero)),
                    new Dialog_ChapterOneEncounter.Choice("再想一想", "可以稍後再與她交談。", () => { })));
            };
            yield return command;
        }
    }
}
