using RimWorld;
using Verse;

namespace CalidoHomecoming
{
    public sealed class ScenPart_CalidoStory : ScenPart
    {
        private static readonly string[] GroundFactionDefNames =
        {
            "Calido_AdventurersGuild",
            "Calido_SlaveCaravan",
            "Calido_BanditCompanies",
            "Calido_MiliraChurch",
            "Calido_FallenCult"
        };

        public override void PreConfigure()
        {
            base.PreConfigure();
            SetGroundFactionGeneration(1);
        }

        public override void PostGameStart()
        {
            base.PostGameStart();
            // World generation has finished. Restore the defs so returning to the
            // main menu and choosing another scenario does not inherit our factions.
            SetGroundFactionGeneration(0);
            Current.Game?.GetComponent<CalidoStoryState>()?.EnableScenario();
        }

        private static void SetGroundFactionGeneration(int count)
        {
            foreach (string defName in GroundFactionDefNames)
            {
                FactionDef factionDef = DefDatabase<FactionDef>.GetNamedSilentFail(defName);
                if (factionDef != null)
                {
                    factionDef.requiredCountAtGameStart = count;
                }
            }
        }

        public override string Summary(Scenario scen)
        {
            return "啟用《卡利多歸航》序幕、南渡留言冊與地面章節。其他劇本和既有存檔不會自動啟動。";
        }
    }
}
