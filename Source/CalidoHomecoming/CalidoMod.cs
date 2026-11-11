using RimWorld;
using HarmonyLib;
using System;
using System.Collections.Generic;
using Verse;

namespace CalidoHomecoming
{
    public sealed class CalidoMod : Mod
    {
        public CalidoMod(ModContentPack content) : base(content)
        {
            Log.Message("[卡利多歸航] 模組框架已載入。");
            new Harmony("asuisui.calidohomecoming").PatchAll();
            LongEventHandler.ExecuteWhenFinished(InstallWaystationSpeed);
            LongEventHandler.ExecuteWhenFinished(InstallAuthorizationBlueprints);
            LongEventHandler.ExecuteWhenFinished(InstallMultiRaceFactionPools);
        }

        private static void InstallWaystationSpeed()
        {
            StatDef move = StatDefOf.MoveSpeed;
            move.parts ??= new List<StatPart>();
            if (!move.parts.Exists(p => p is StatPart_WaystationHauling))
                move.parts.Add(new StatPart_WaystationHauling { parentStat = move });
        }

        private static void InstallMultiRaceFactionPools()
        {
            InstallKingdomPopulation("Calido_MihoKingdom", "Miho_Faction_Megacorp", "Miho_Soldier_Militia", "美狐");
            InstallKingdomPopulation("Calido_KiiroKingdom", "Kiiro_Faction", "Kiiro_Colonist", "綺羅");
            InstallKingdomPopulation("Calido_WolfeinKingdom", "Wolfein_Faction_MegaCorp", "Wolfein_MegaCorpCivilian", "沃芬");
            InstallKingdomPopulation("Calido_AxolotlRiverfolk", "AxolotlWanderingDynasty", "AxolotlCombatant_Physics", "萌螈");
            string[] terrestrialKinds =
            {
                "Miho_Colonist", "Miho_Soldier_Militia", "Miho_Soldier_Hunter",
                "Kiiro_Colonist", "Kiiro_Militia", "Kiiro_Guerrilla",
                "Wolfein_Colonist", "Wolfein_MegaCorpCivilian", "Wolfein_RebelSoldier",
                "Axolotl_CultDisciple", "Axolotl_CrimsonBloodGuardian"
            };
            string[] groundFactions =
            {
                "Calido_AdventurersGuild", "Calido_SlaveCaravan", "Calido_BanditCompanies",
                "Calido_MiliraChurch", "Calido_FallenCult"
            };
            int inserted = 0;
            foreach (string factionName in groundFactions)
            {
                FactionDef faction = DefDatabase<FactionDef>.GetNamedSilentFail(factionName);
                if (faction?.pawnGroupMakers == null) continue;
                foreach (PawnGroupMaker maker in faction.pawnGroupMakers)
                {
                    maker.options ??= new List<PawnGenOption>();
                    foreach (string kindName in terrestrialKinds)
                    {
                        PawnKindDef kind = DefDatabase<PawnKindDef>.GetNamedSilentFail(kindName);
                        if (kind == null || maker.options.Exists(o => o.kind == kind)) continue;
                        maker.options.Add(new PawnGenOption { kind = kind, selectionWeight = factionName == "Calido_AdventurersGuild" ? 0.35f : 0.18f });
                        inserted++;
                    }

                    // The guild alone contains a small number of ordinary ground Milira.
                    // The cult receives an even rarer fallen individual; neither can roll
                    // Calido high-tech gear because the scenario lock strips it.
                    if (factionName == "Calido_AdventurersGuild" || factionName == "Calido_FallenCult")
                    {
                        PawnKindDef milira = DefDatabase<PawnKindDef>.GetNamedSilentFail("Milira_Colonist");
                        if (milira != null && !maker.options.Exists(o => o.kind == milira))
                        {
                            maker.options.Add(new PawnGenOption { kind = milira, selectionWeight = factionName == "Calido_FallenCult" ? 0.04f : 0.10f });
                            inserted++;
                        }
                    }
                }
            }
            Log.Message($"[卡利多歸航] 已向地面陣營加入 {inserted} 項可用的跨種族生成選項；未安裝的種族會自動略過。");
        }

        private static void InstallKingdomPopulation(string kingdomName, string sourceName, string memberName, string raceLabel)
        {
            FactionDef kingdom = DefDatabase<FactionDef>.GetNamedSilentFail(kingdomName);
            FactionDef source = DefDatabase<FactionDef>.GetNamedSilentFail(sourceName);
            if (kingdom == null || source?.pawnGroupMakers == null) return;
            kingdom.pawnGroupMakers = source.pawnGroupMakers;
            kingdom.basicMemberKind = DefDatabase<PawnKindDef>.GetNamedSilentFail(memberName) ?? source.basicMemberKind;
            kingdom.fixedLeaderKinds = source.fixedLeaderKinds;
            if (kingdom.fixedLeaderKinds.NullOrEmpty() && kingdom.basicMemberKind != null)
                kingdom.fixedLeaderKinds = new List<PawnKindDef> { kingdom.basicMemberKind };
            // The Biotech tooltip reports xenotypes, while these four species are alien races.
            kingdom.xenotypeSet = source.xenotypeSet;
            kingdom.description += "\n\n主要居民：" + raceLabel + "族。商隊、守軍與護衛使用本族原陣營的人員編制。";
        }

        private static void InstallAuthorizationBlueprints()
        {
            ResearchProjectDef civilian = DefDatabase<ResearchProjectDef>.GetNamedSilentFail("Calido_Blueprint_Civilian");
            ResearchProjectDef armor = DefDatabase<ResearchProjectDef>.GetNamedSilentFail("Calido_Blueprint_Armor");
            ResearchProjectDef weapons = DefDatabase<ResearchProjectDef>.GetNamedSilentFail("Calido_Blueprint_Weapons");
            ResearchProjectDef milian = DefDatabase<ResearchProjectDef>.GetNamedSilentFail("Calido_Blueprint_Milian");
            int patched = 0;
            foreach (ResearchProjectDef project in DefDatabase<ResearchProjectDef>.AllDefsListForReading)
            {
                if (!IsLockedMiliraResearch(project)) continue;
                ResearchProjectDef blueprint = SelectBlueprint(project.defName, civilian, armor, weapons, milian);
                if (blueprint == null || project == blueprint) continue;
                project.prerequisites ??= new List<ResearchProjectDef>();
                if (!project.prerequisites.Contains(blueprint))
                {
                    project.prerequisites.Add(blueprint);
                    blueprint.requiredByThis ??= new List<ResearchProjectDef>();
                    if (!blueprint.requiredByThis.Contains(project)) blueprint.requiredByThis.Add(project);
                    patched++;
                }
            }
            Log.Message($"[卡利多歸航] 已用四類授權藍圖封鎖 {patched} 個米莉拉高科技研究節點。低階地面科技不受影響。");
        }

        private static bool IsLockedMiliraResearch(ResearchProjectDef project)
        {
            if (project?.defName == null || !project.defName.StartsWith("Milira_", StringComparison.OrdinalIgnoreCase)) return false;
            return project.techLevel >= TechLevel.Spacer || project.defName.StartsWith("Milira_MilianTech_", StringComparison.OrdinalIgnoreCase);
        }

        private static ResearchProjectDef SelectBlueprint(string name, ResearchProjectDef civilian, ResearchProjectDef armor, ResearchProjectDef weapons, ResearchProjectDef milian)
        {
            if (name.IndexOf("Milian", StringComparison.OrdinalIgnoreCase) >= 0 || name.IndexOf("Drone", StringComparison.OrdinalIgnoreCase) >= 0 || name.IndexOf("FloatUnit", StringComparison.OrdinalIgnoreCase) >= 0) return milian;
            if (name.IndexOf("Apparel", StringComparison.OrdinalIgnoreCase) >= 0 || name.IndexOf("Shield", StringComparison.OrdinalIgnoreCase) >= 0 || name.IndexOf("Field", StringComparison.OrdinalIgnoreCase) >= 0) return armor;
            string[] weaponWords = { "Weapon", "Blade", "Sword", "Spear", "Lance", "Hammer", "Sickle", "RailGun", "ParticleAccelerator" };
            foreach (string word in weaponWords) if (name.IndexOf(word, StringComparison.OrdinalIgnoreCase) >= 0) return weapons;
            return civilian;
        }
    }
}
