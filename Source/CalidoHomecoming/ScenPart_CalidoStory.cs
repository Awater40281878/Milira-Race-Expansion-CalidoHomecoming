using RimWorld;
using HarmonyLib;
using System.Linq;
using System.Collections.Generic;
using Verse;

namespace CalidoHomecoming
{
    public sealed class ScenPart_CalidoStory : ScenPart
    {
        private static readonly string[] SurfaceFactionDefNames =
        {
            "Calido_AdventurersGuild",
            "Calido_SlaveCaravan",
            "Calido_BanditCompanies",
            "Calido_MiliraChurch",
            "Calido_FallenCult",
            "Calido_MihoKingdom",
            "Calido_KiiroKingdom",
            "Calido_WolfeinKingdom",
            "Calido_AxolotlRiverfolk"
        };

        private static readonly string[] NonSurfaceFactionDefNames =
        {
            "Mechanoid", "Insect", "Insects",
            "Calido_OrbitalTradeConsortium", "Calido_OrbitalPirates", "Calido_OrbitalMilira"
        };

        internal static bool StorySelected => Find.Scenario?.AllParts.Any(p => p is ScenPart_CalidoStory) == true;

        internal static List<FactionDef> Roster() => SurfaceFactionDefNames.Concat(NonSurfaceFactionDefNames)
            .Select(n => DefDatabase<FactionDef>.GetNamedSilentFail(n)).Where(f => f != null).Distinct().ToList();

        public override void PostGameStart()
        {
            base.PostGameStart();
            Current.Game?.GetComponent<CalidoStoryState>()?.EnableScenario();
        }

        public override string Summary(Scenario scen)
        {
            return "啟用《卡利多歸航》序幕、南渡留言冊與地面章節。其他劇本和既有存檔不會自動啟動。";
        }
    }

    [HarmonyPatch(typeof(Page_CreateWorldParams), "ResetFactionCounts")]
    internal static class CalidoResetRoster
    {
        static void Postfix(ref List<FactionDef> ___factions)
        {
            if (ScenPart_CalidoStory.StorySelected) ___factions = ScenPart_CalidoStory.Roster();
        }
    }

    [HarmonyPatch(typeof(Page_CreateWorldParams), "DoWindowContents")]
    internal static class CalidoWorldRoster
    {
        static void Prefix(ref List<FactionDef> ___factions)
        {
            if (!ScenPart_CalidoStory.StorySelected) return;
            var roster = ScenPart_CalidoStory.Roster();
            if (___factions == null || !___factions.SequenceEqual(roster)) ___factions = roster;
        }
    }

    [HarmonyPatch(typeof(FactionGenerator), "get_ConfigurableFactions")]
    internal static class CalidoFactionOptions
    {
        static void Postfix(ref IEnumerable<FactionDef> __result)
        {
            if (Current.ProgramState == ProgramState.Entry && ScenPart_CalidoStory.StorySelected)
                __result = __result.Where(f => ScenPart_CalidoStory.Roster().Contains(f));
        }
    }
}
