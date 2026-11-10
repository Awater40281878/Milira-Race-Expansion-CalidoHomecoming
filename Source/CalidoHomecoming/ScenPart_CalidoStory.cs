using RimWorld;
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

        private sealed class FactionGenerationSettings
        {
            public int RequiredCount;
            public float SettlementWeight;
        }

        private static readonly Dictionary<FactionDef, FactionGenerationSettings> OriginalGenerationSettings = new Dictionary<FactionDef, FactionGenerationSettings>();

        public override void PreConfigure()
        {
            base.PreConfigure();
            ConfigureStoryFactionGeneration();
        }

        public override void PostGameStart()
        {
            base.PostGameStart();
            // World generation has finished. Restore the defs so returning to the
            // main menu and choosing another scenario does not inherit our factions.
            RestoreFactionGeneration();
            Current.Game?.GetComponent<CalidoStoryState>()?.EnableScenario();
        }

        private static void ConfigureStoryFactionGeneration()
        {
            OriginalGenerationSettings.Clear();
            foreach (FactionDef factionDef in DefDatabase<FactionDef>.AllDefsListForReading)
            {
                if (factionDef == null || factionDef.isPlayer) continue;
                OriginalGenerationSettings[factionDef] = new FactionGenerationSettings
                {
                    RequiredCount = factionDef.requiredCountAtGameStart,
                    SettlementWeight = factionDef.settlementGenerationWeight
                };
                factionDef.requiredCountAtGameStart = 0;
            }

            foreach (string defName in SurfaceFactionDefNames)
                SetFactionGeneration(defName, 1, null);
            foreach (string defName in NonSurfaceFactionDefNames)
                SetFactionGeneration(defName, 1, 0f);
        }

        private static void SetFactionGeneration(string defName, int requiredCount, float? settlementWeight)
        {
            FactionDef factionDef = DefDatabase<FactionDef>.GetNamedSilentFail(defName);
            if (factionDef == null || factionDef.isPlayer) return;
            factionDef.requiredCountAtGameStart = requiredCount;
            if (settlementWeight.HasValue) factionDef.settlementGenerationWeight = settlementWeight.Value;
        }

        private static void RestoreFactionGeneration()
        {
            foreach (KeyValuePair<FactionDef, FactionGenerationSettings> pair in OriginalGenerationSettings)
            {
                pair.Key.requiredCountAtGameStart = pair.Value.RequiredCount;
                pair.Key.settlementGenerationWeight = pair.Value.SettlementWeight;
            }
            OriginalGenerationSettings.Clear();
        }

        public override string Summary(Scenario scen)
        {
            return "啟用《卡利多歸航》序幕、南渡留言冊與地面章節。其他劇本和既有存檔不會自動啟動。";
        }
    }
}
