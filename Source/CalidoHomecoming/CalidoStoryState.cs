using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace CalidoHomecoming
{
    public enum CalidoPrologueChoice
    {
        None,
        GiveName,
        BorrowName,
        RefuseName
    }

    public enum CalidoFeatherChoice
    {
        None,
        ReturnIt,
        KeepAsCommonMemory,
        ForbidWorship
    }

    public sealed class CalidoStoryState : GameComponent
    {
        private bool scenarioEnabled;
        private bool prologueQueued;
        private bool prologueComplete;
        private int prologueStartTick = -1;
        private CalidoPrologueChoice prologueChoice;
        private bool featherEventQueued;
        private bool featherEventComplete;
        private int featherEventStartTick = -1;
        private CalidoFeatherChoice featherChoice;
        private bool messagePostQuestAnnounced;
        private bool messagePostQuestComplete;
        private int messagePostQuestStartTick = -1;
        private bool groundWorldSanitized;
        private bool miliraDiplomacyInitialized;
        private bool factionDiplomacyInitialized;
        private int chapterOneStep;
        private int chapterOneNextTick = -1;
        private bool chapterOneWindowOpen;
        private bool firstHeroRecruited;
        private bool chapterOneComplete;
        private int campaignBeat;
        private int campaignNextTick = -1;
        private bool campaignWindowOpen;
        private int activeCampaignBeat = -1;
        private int activeCampaignResolveTick = -1;
        private bool activeCampaignHumane;
        private bool activeCampaignCombat;
        private bool calidoTechUnlocked;
        private bool cultResurrectionUnlocked;
        private bool returningLanternGranted;
        private bool returningLanternEvolutionReady;
        private bool returningLanternReminderSent;
        private int protagonistId = -1;
        private bool fallenFormActive;
        private int fallenFormEndTick = -1;
        private float fallenFormStartHealth = 1f;
        private List<int> crossRaceEventsSeen = new List<int>();
        private int nextCrossRaceEventTick = -1;
        private bool crossRaceEventWindowOpen;
        private int nextGuildPrisonerVisitTick = -1;
        private bool guildPrisonerWindowOpen;
        private bool cultActivated;
        private bool cultDiplomacyInitialized;
        private bool startingReachSpearGranted;

        public CalidoStoryState(Game game)
        {
        }

        public bool PrologueQueued => prologueQueued;
        public bool ScenarioEnabled => scenarioEnabled;
        public bool PrologueComplete => prologueComplete;
        public CalidoPrologueChoice PrologueChoice => prologueChoice;
        public bool FeatherEventComplete => featherEventComplete;
        public CalidoFeatherChoice FeatherChoice => featherChoice;
        public bool MessagePostQuestAnnounced => messagePostQuestAnnounced;
        public bool MessagePostQuestComplete => messagePostQuestComplete;
        public int ChapterOneStep => chapterOneStep;
        public bool FirstHeroRecruited => firstHeroRecruited;
        public bool ChapterOneComplete => chapterOneComplete;
        public int CampaignBeat => campaignBeat;
        public int ActiveCampaignBeat => activeCampaignBeat;
        public bool ActiveCampaignCombat => activeCampaignCombat;
        public bool CalidoTechUnlocked => calidoTechUnlocked;
        public bool CultResurrectionUnlocked => cultResurrectionUnlocked;
        public bool WeaponBlueprintUnlocked => BlueprintFinished("Calido_Blueprint_Weapons");
        public bool ArmorBlueprintUnlocked => BlueprintFinished("Calido_Blueprint_Armor");
        public bool CivilianBlueprintUnlocked => BlueprintFinished("Calido_Blueprint_Civilian");
        public bool MilianBlueprintUnlocked => DefDatabase<ResearchProjectDef>.GetNamedSilentFail("Calido_Blueprint_Milian") == null || BlueprintFinished("Calido_Blueprint_Milian");
        public int CrossRaceEventsSeen => crossRaceEventsSeen?.Count ?? 0;

        public override void StartedNewGame()
        {
            // The scenario part enables the story in PostGameStart. Keeping the
            // component dormant here prevents all other scenarios from changing.
        }

        public override void LoadedGame()
        {
            if (scenarioEnabled && !prologueComplete && prologueStartTick < 0)
            {
                prologueStartTick = (Find.TickManager?.TicksGame ?? 0) + 120;
            }
        }

        public override void GameComponentTick()
        {
            if (!scenarioEnabled)
            {
                return;
            }

            SanitizeGroundWorld();
            InitializeMiliraDiplomacy();
            InitializeFactionDiplomacy();
            TickCultDiplomacy();
            EnsureAuthorizationBlueprints();
            EnsureStartingReachSpear();
            EnsureProtagonistAura();

            if (messagePostQuestComplete)
            {
                TickChapterOne();
            }
            TickExtendedCampaign();
            TickTechnologyLock();
            TickFallenResurrection();
            TickCrossRaceEvents();
            TickGuildPrisonerBuyer();

            if (prologueComplete || prologueQueued || Find.TickManager == null)
            {
                TickFeatherEvent();
                return;
            }

            if (prologueStartTick < 0)
            {
                prologueStartTick = Find.TickManager.TicksGame + 600;
            }

            if (Find.TickManager.TicksGame < prologueStartTick || Find.AnyPlayerHomeMap == null)
            {
                return;
            }

            prologueQueued = true;
            Find.WindowStack.Add(new Dialog_BorrowedName(this));
        }

        public void EnableScenario()
        {
            if (scenarioEnabled)
            {
                return;
            }

            scenarioEnabled = true;
            prologueStartTick = (Find.TickManager?.TicksGame ?? 0) + 600;
            Log.Message("[卡利多歸航] 已由專用劇本啟動。其他存檔不受影響。");
        }

        private void SanitizeGroundWorld()
        {
            if (groundWorldSanitized || Find.WorldObjects == null)
            {
                return;
            }

            // The ground chapter may contain individual Milira pawns, but never a
            // Milira nation or settlement. Calido itself is reached by story later.
            // This is deliberately scenario-gated so existing saves are untouched.
            FactionDef miliraFactionDef = DefDatabase<FactionDef>.GetNamedSilentFail("Milira_Faction");
            if (miliraFactionDef == null)
            {
                return;
            }

            int removed = 0;
            var settlements = Find.WorldObjects.Settlements;
            for (int i = settlements.Count - 1; i >= 0; i--)
            {
                Settlement settlement = settlements[i];
                if (settlement?.Faction?.def == miliraFactionDef)
                {
                    settlement.Destroy();
                    removed++;
                }
            }

            groundWorldSanitized = true;
            Log.Message($"[卡利多歸航] 地面世界規則完成：已移除 {removed} 座米莉拉地面據點。個別米莉拉仍可由劇情生成。");
        }

        private void InitializeMiliraDiplomacy()
        {
            if (Find.FactionManager == null || Faction.OfPlayer == null)
            {
                return;
            }

            FactionDef miliraDef = DefDatabase<FactionDef>.GetNamedSilentFail("Milira_Faction");
            Faction miliraFaction = Find.FactionManager.AllFactions.FirstOrDefault(f => f.def.defName == "Calido_OrbitalMilira")
                ?? (miliraDef == null ? null : Find.FactionManager.AllFactions.FirstOrDefault(f => f.def == miliraDef));
            if (miliraFaction == null)
            {
                return;
            }

            // Milira Race normally treats every faction except Milira_PlayerFaction
            // as a permanent enemy. This scenario is intentionally multi-racial, so
            // the ordinary player faction receives an exception without replacing it.
            if (miliraDef?.permanentEnemyToEveryoneExcept != null &&
                !miliraDef.permanentEnemyToEveryoneExcept.Contains(Faction.OfPlayer.def))
            {
                miliraDef.permanentEnemyToEveryoneExcept.Add(Faction.OfPlayer.def);
            }

            // The exception lives on a Def and must be restored after every game
            // process restart. The saved flag only prevents overwriting diplomacy
            // changes made later by the player's actions.
            if (miliraDiplomacyInitialized)
            {
                return;
            }

            miliraFaction.SetRelationDirect(Faction.OfPlayer, FactionRelationKind.Neutral);
            Faction.OfPlayer.SetRelationDirect(miliraFaction, FactionRelationKind.Neutral);
            miliraDiplomacyInitialized = true;
            Log.Message("[卡利多歸航] 卡利多天控城與玩家的初始外交已改為中立；登城前仍無法直接聯絡。");
        }

        private void InitializeFactionDiplomacy()
        {
            if (factionDiplomacyInitialized || Faction.OfPlayer == null || Find.FactionManager == null) return;

            // Player-facing relations are deterministic; the cult is changed later by its arc.
            SetPlayerRelation("Calido_AdventurersGuild", FactionRelationKind.Ally);
            SetPlayerRelation("Calido_MiliraChurch", FactionRelationKind.Ally);
            SetPlayerRelation("Calido_MihoKingdom", FactionRelationKind.Neutral);
            SetPlayerRelation("Calido_KiiroKingdom", FactionRelationKind.Neutral);
            SetPlayerRelation("Calido_WolfeinKingdom", FactionRelationKind.Neutral);
            SetPlayerRelation("Calido_AxolotlRiverfolk", FactionRelationKind.Neutral);
            SetPlayerRelation("Calido_OrbitalMilira", FactionRelationKind.Neutral);
            SetPlayerRelation("Calido_OrbitalTradeConsortium", FactionRelationKind.Neutral);
            SetPlayerRelation("Calido_FallenCult", FactionRelationKind.Neutral);
            SetPlayerRelation("Calido_SlaveCaravan", FactionRelationKind.Hostile);
            SetPlayerRelation("Calido_BanditCompanies", FactionRelationKind.Hostile);
            SetPlayerRelation("Calido_OrbitalPirates", FactionRelationKind.Hostile);
            SetPlayerRelation("Mechanoid", FactionRelationKind.Hostile);
            SetPlayerRelation("Insect", FactionRelationKind.Hostile);

            string[] kingdoms = { "Calido_MihoKingdom", "Calido_KiiroKingdom", "Calido_WolfeinKingdom", "Calido_AxolotlRiverfolk" };
            for (int i = 0; i < kingdoms.Length; i++)
            {
                SetFactionRelation("Calido_AdventurersGuild", kingdoms[i], FactionRelationKind.Ally);
                SetFactionRelation("Calido_MiliraChurch", kingdoms[i], FactionRelationKind.Ally);
                for (int j = i + 1; j < kingdoms.Length; j++) SetFactionRelation(kingdoms[i], kingdoms[j], FactionRelationKind.Neutral);
            }

            SetFactionRelation("Calido_AdventurersGuild", "Calido_MiliraChurch", FactionRelationKind.Ally);
            SetFactionRelation("Calido_AdventurersGuild", "Calido_SlaveCaravan", FactionRelationKind.Hostile);
            SetFactionRelation("Calido_AdventurersGuild", "Calido_BanditCompanies", FactionRelationKind.Hostile);
            SetFactionRelation("Calido_AdventurersGuild", "Calido_FallenCult", FactionRelationKind.Hostile);
            SetFactionRelation("Calido_MiliraChurch", "Calido_SlaveCaravan", FactionRelationKind.Hostile);
            SetFactionRelation("Calido_MiliraChurch", "Calido_BanditCompanies", FactionRelationKind.Hostile);
            SetFactionRelation("Calido_MiliraChurch", "Calido_FallenCult", FactionRelationKind.Hostile);
            foreach (string kingdom in kingdoms)
            {
                SetFactionRelation(kingdom, "Calido_SlaveCaravan", FactionRelationKind.Hostile);
                SetFactionRelation(kingdom, "Calido_BanditCompanies", FactionRelationKind.Hostile);
                SetFactionRelation(kingdom, "Calido_FallenCult", FactionRelationKind.Neutral);
            }
            SetFactionRelation("Calido_SlaveCaravan", "Calido_BanditCompanies", FactionRelationKind.Neutral);
            SetFactionRelation("Calido_SlaveCaravan", "Calido_FallenCult", FactionRelationKind.Neutral);
            SetFactionRelation("Calido_BanditCompanies", "Calido_FallenCult", FactionRelationKind.Neutral);

            SetFactionRelation("Calido_OrbitalMilira", "Calido_OrbitalTradeConsortium", FactionRelationKind.Ally);
            SetFactionRelation("Calido_OrbitalMilira", "Calido_OrbitalPirates", FactionRelationKind.Hostile);
            SetFactionRelation("Calido_OrbitalTradeConsortium", "Calido_OrbitalPirates", FactionRelationKind.Hostile);
            foreach (string surfaceFaction in kingdoms.Concat(new[] { "Calido_AdventurersGuild", "Calido_MiliraChurch", "Calido_SlaveCaravan", "Calido_BanditCompanies", "Calido_FallenCult" }))
            {
                SetFactionRelation("Calido_OrbitalTradeConsortium", surfaceFaction, FactionRelationKind.Neutral);
                SetFactionRelation("Calido_OrbitalPirates", surfaceFaction, FactionRelationKind.Hostile);
            }

            factionDiplomacyInitialized = true;
            Log.Message("[卡利多歸航] 已套用地面與軌道陣營外交矩陣。密教會在偽神諭案推進後對玩家轉為敵對。");
        }

        private static void SetPlayerRelation(string factionDefName, FactionRelationKind relation)
        {
            Faction faction = Find.FactionManager.AllFactions.FirstOrDefault(f => f.def.defName == factionDefName);
            if (faction == null || faction == Faction.OfPlayer) return;
            faction.SetRelationDirect(Faction.OfPlayer, relation);
            Faction.OfPlayer.SetRelationDirect(faction, relation);
        }

        private static void SetFactionRelation(string firstDefName, string secondDefName, FactionRelationKind relation)
        {
            Faction first = Find.FactionManager.AllFactions.FirstOrDefault(f => f.def.defName == firstDefName);
            Faction second = Find.FactionManager.AllFactions.FirstOrDefault(f => f.def.defName == secondDefName);
            if (first == null || second == null || first == second) return;
            first.SetRelationDirect(second, relation);
            second.SetRelationDirect(first, relation);
        }

        private void TickCultDiplomacy()
        {
            if (Find.FactionManager == null || Faction.OfPlayer == null) return;
            Faction cult = Find.FactionManager.AllFactions.FirstOrDefault(f => f.def.defName == "Calido_FallenCult");
            if (cult == null) return;
            if (!cultActivated && campaignBeat < 16)
            {
                if (!cultDiplomacyInitialized || cult.HostileTo(Faction.OfPlayer))
                {
                    cult.SetRelationDirect(Faction.OfPlayer, FactionRelationKind.Neutral);
                    Faction.OfPlayer.SetRelationDirect(cult, FactionRelationKind.Neutral);
                    cultDiplomacyInitialized = true;
                }
                return;
            }
            if (!cultActivated && campaignBeat >= 16)
            {
                cultActivated = true;
                cult.SetRelationDirect(Faction.OfPlayer, FactionRelationKind.Hostile);
                Faction.OfPlayer.SetRelationDirect(cult, FactionRelationKind.Hostile);
                Find.LetterStack.ReceiveLetter("蝕羽密教公開敵對", "你開始追查抄經所後，密教不再維持表面中立。從現在起，他們會出現在正常襲擊池中，且仍會先依地面交涉規則提出條件。", LetterDefOf.ThreatBig);
            }
        }

        private void TickFeatherEvent()
        {
            if (!prologueComplete || Find.TickManager == null || Find.AnyPlayerHomeMap == null)
            {
                return;
            }

            if (featherEventComplete)
            {
                TickMessagePostQuest();
                return;
            }

            if (featherEventQueued)
            {
                return;
            }

            if (featherEventStartTick < 0)
            {
                featherEventStartTick = Find.TickManager.TicksGame + 1200;
            }

            if (Find.TickManager.TicksGame < featherEventStartTick)
            {
                return;
            }

            featherEventQueued = true;
            Find.WindowStack.Add(new Dialog_FeatherAtWell(this));
        }

        private void EnsureStartingReachSpear()
        {
            if (startingReachSpearGranted || Find.AnyPlayerHomeMap == null) return;
            Pawn protagonist = Find.AnyPlayerHomeMap.mapPawns.FreeColonistsSpawned.FirstOrDefault(IsMilira);
            ThingDef spearDef = DefDatabase<ThingDef>.GetNamedSilentFail("Calido_ReachSpear");
            if (protagonist == null || spearDef == null || protagonist.equipment == null) return;

            ThingWithComps oldWeapon = protagonist.equipment.Primary;
            if (oldWeapon != null)
                protagonist.equipment.TryDropEquipment(oldWeapon, out _, protagonist.Position, forbid: false);
            protagonist.equipment.AddEquipment((ThingWithComps)ThingMaker.MakeThing(spearDef));
            protagonistId = protagonist.thingIDNumber;
            startingReachSpearGranted = true;
            Messages.Message("主角裝備了普通地面武器「渡口長槍」。它能在4.9格內延伸刺擊，也可用於狩獵。", protagonist, MessageTypeDefOf.PositiveEvent, false);
        }

        private void EnsureProtagonistAura()
        {
            if (protagonistId < 0) return;

            Pawn protagonist = Find.Maps.SelectMany(m => m.mapPawns.FreeColonistsSpawned)
                .FirstOrDefault(p => p.thingIDNumber == protagonistId);
            TraitDef aura = DefDatabase<TraitDef>.GetNamedSilentFail("Calido_ProtagonistAura");
            if (protagonist?.story?.traits == null || aura == null) return;

            if (!protagonist.story.traits.HasTrait(aura))
            {
                protagonist.story.traits.GainTrait(new Trait(aura));
                Messages.Message("主角獲得特質「主角光環」：忍痛、免疫力與魅力提高。", protagonist, MessageTypeDefOf.PositiveEvent, false);
            }

            ThoughtDef auraMood = DefDatabase<ThoughtDef>.GetNamedSilentFail("Calido_ProtagonistAuraMood");
            var memories = protagonist.needs?.mood?.thoughts?.memories;
            if (auraMood != null && memories != null && memories.NumMemoriesOfDef(auraMood) == 0)
                memories.TryGainMemory(auraMood);
        }

        private void TickMessagePostQuest()
        {
            if (messagePostQuestComplete || Find.TickManager == null)
            {
                return;
            }

            if (messagePostQuestStartTick < 0)
            {
                messagePostQuestStartTick = Find.TickManager.TicksGame + 600;
            }

            if (!messagePostQuestAnnounced && Find.TickManager.TicksGame >= messagePostQuestStartTick)
            {
                messagePostQuestAnnounced = true;
                Find.LetterStack.ReceiveLetter(
                    "委託：為南渡立一座留言柱",
                    "井邊的爭論讓居民發現，口耳相傳會把每句話都變成神諭。他們請你在殖民地建立一座普通木製留言柱。\n\n請從「雜項」建築分類建造「南渡留言柱」。需要 40 單位木材；任何殖民者都能施工，不需要研究或高科技。",
                    LetterDefOf.PositiveEvent);
            }

            if (!messagePostQuestAnnounced || Find.TickManager.TicksGame % 120 != 0)
            {
                return;
            }

            ThingDef postDef = DefDatabase<ThingDef>.GetNamedSilentFail("Calido_MessagePost");
            if (postDef == null)
            {
                return;
            }

            foreach (Map map in Find.Maps)
            {
                if (!map.IsPlayerHome)
                {
                    continue;
                }

                if (map.listerThings.ThingsOfDef(postDef).Count > 0)
                {
                    messagePostQuestComplete = true;
                    chapterOneNextTick = Find.TickManager.TicksGame + 1200;
                    Find.LetterStack.ReceiveLetter(
                        "南渡留言柱完工",
                        "木板沒有發光，紙頁也不會自行回答問題。正因如此，居民願意把真正需要幫忙的事情寫在上面。\n\n第一個建設任務已完成；留言冊現在擁有實體的地面入口。",
                        LetterDefOf.PositiveEvent,
                        map.listerThings.ThingsOfDef(postDef)[0]);
                    break;
                }
            }
        }

        private void TickChapterOne()
        {
            if (chapterOneComplete || chapterOneWindowOpen || Find.TickManager == null || Find.AnyPlayerHomeMap == null)
            {
                return;
            }

            if (chapterOneNextTick < 0)
            {
                chapterOneNextTick = Find.TickManager.TicksGame + 1200;
            }

            if (Find.TickManager.TicksGame < chapterOneNextTick)
            {
                return;
            }

            chapterOneWindowOpen = true;
            switch (chapterOneStep)
            {
                case 0: OpenGuildCommission(); break;
                case 1: OpenHeroRecruitment(); break;
                case 2: OpenBanditNegotiation(); break;
                case 3: OpenSlaveRoadEncounter(); break;
                case 4: OpenCalidoClue(); break;
                default: CompleteChapterOne(); break;
            }
        }

        private void OpenGuildCommission()
        {
            Find.WindowStack.Add(new Dialog_ChapterOneEncounter(
                "公會委託：失蹤的記錄員", "冒險者公會・南渡代理人",
                "留言柱立起後，第一張正式委託來自冒險者公會。一名替公會核對罪犯名冊的記錄員在北側古道失蹤。公會不要求你相信任何神諭，只希望你提供補給，讓搜索隊能在暴雨前出發。",
                new Dialog_ChapterOneEncounter.Choice("提供 80 銀", "立即支付；公會會記住這次合作。", () => ResolveGuild(true), () => ColonyHas(ThingDefOf.Silver, 80)),
                new Dialog_ChapterOneEncounter.Choice("提供糧食與嚮導", "不扣銀，以殖民地的時間與口糧代替。", () => ResolveGuild(false)),
                new Dialog_ChapterOneEncounter.Choice("拒絕介入", "仍會推進故事，但公會不會給予額外報酬。", () => AdvanceChapter("公會自行派出了搜索隊。南渡沒有因此成為敵人，但第一份合作紀錄留了空白。"))));
        }

        private void ResolveGuild(bool paid)
        {
            if (paid) ConsumeFromColony(ThingDefOf.Silver, 80);
            Thing reward = ThingMaker.MakeThing(ThingDefOf.MedicineIndustrial);
            reward.stackCount = paid ? 8 : 5;
            GenPlace.TryPlaceThing(reward, Find.AnyPlayerHomeMap.Center, Find.AnyPlayerHomeMap, ThingPlaceMode.Near);
            AdvanceChapter(paid
                ? "搜索隊在舊橋下找到了受傷的記錄員。公會送來八份工業藥物，並將南渡列為可信委託點。"
                : "你的嚮導把搜索隊帶過漲水的支流。記錄員獲救，公會送來五份工業藥物。" );
        }

        private void OpenHeroRecruitment()
        {
            Find.WindowStack.Add(new Dialog_ChapterOneEncounter(
                "第一位英雄：伊蕾希雅", "白翼旅人・伊蕾希雅",
                "獲救的記錄員身旁還有一名用舊布包住翅膀的米莉拉。她自稱伊蕾希雅，是失去天空航標後在地面替人運送物資的遠棧。她沒有卡利多裝備，只有磨損的旅行衣與一張寫滿道路的地圖。\n\n「如果妳也在找回去的路，我可以先替這裡的人搬東西。」",
                new Dialog_ChapterOneEncounter.Choice("邀請她加入", "伊蕾希雅將成為殖民者。", RecruitFirstHero),
                new Dialog_ChapterOneEncounter.Choice("請她先留在南渡", "她仍會加入，但以觀察者身分開始。", RecruitFirstHero),
                new Dialog_ChapterOneEncounter.Choice("拒絕", "她會離開；之後可由公會支線再次招募。", () => AdvanceChapter("伊蕾希雅收起地圖離開南渡。公會答應日後替你轉交消息。"))));
        }

        private void RecruitFirstHero()
        {
            PawnKindDef kind = DefDatabase<PawnKindDef>.GetNamedSilentFail("Milira_Colonist");
            Map map = Find.AnyPlayerHomeMap;
            if (kind != null && map != null)
            {
                Pawn pawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(kind, Faction.OfPlayer));
                pawn.Name = new NameTriple("伊蕾希雅", "遠棧", "阿斯特里昂");
                HediffDef specialty = DefDatabase<HediffDef>.GetNamedSilentFail("Calido_Hero_Waystation");
                if (specialty != null)
                {
                    pawn.health.AddHediff(specialty);
                }
                GenSpawn.Spawn(pawn, CellFinder.RandomClosewalkCellNear(map.Center, map, 8), map);
                firstHeroRecruited = true;
            }
            AdvanceChapter("伊蕾希雅把地圖釘在留言柱背面。她是第一位加入南渡的米莉拉英雄，也是卡利多失聯並非孤例的第一份證據。" );
        }

        private void OpenBanditNegotiation()
        {
            Find.WindowStack.Add(new Dialog_ChapterOneEncounter(
                "路障交涉", "荒路強盜・赤索頭目",
                "一隊武裝強盜停在射程外，先派人送來條件：一百五十銀、全部工業藥物，再讓「有翅膀的那個」去營地證明她是不是神。這是一份刻意讓人無法接受的要求。",
                new Dialog_ChapterOneEncounter.Choice("只付 100 銀", "提出一次有底線的買路費。", () => { ConsumeFromColony(ThingDefOf.Silver, 100); AdvanceChapter("強盜收下銀幣撤走，卻留下刻有斷翼圖樣的木牌。那不是普通路標。" ); }, () => ColonyHas(ThingDefOf.Silver, 100)),
                new Dialog_ChapterOneEncounter.Choice("公開拒絕", "談判破裂，荒路強盜將發動襲擊。", () => { TriggerRaid("Calido_BanditCompanies"); AdvanceChapter("你拒絕把任何人交出去。赤索頭目下令進攻；南渡第一次用自己的名字守住道路。" ); }),
                new Dialog_ChapterOneEncounter.Choice("以斷翼木牌反問", "指出他們並非第一次追捕白翼者，使對方暫時撤退。", () => AdvanceChapter("你認出使者腰間也掛著斷翼木牌。謊言被揭穿後，強盜暫時撤退；木牌指向一條奴隸商路。"))));
        }

        private void OpenSlaveRoadEncounter()
        {
            Find.WindowStack.Add(new Dialog_ChapterOneEncounter(
                "鎖鏈商路", "鎖鏈商路・押運官",
                "一支軍容整齊的押運隊沿斷翼標記而來。他們聲稱車中的亞人與孩童都是合法財產，並要求南渡交還一名逃亡的抄經員。對方願意先談，因為公開戰鬥會損害貨物。",
                new Dialog_ChapterOneEncounter.Choice("支付 120 銀贖人", "贖下俘虜並取得抄經員保存的紙片。", () => { ConsumeFromColony(ThingDefOf.Silver, 120); AdvanceChapter("押運官收錢後放下兩名俘虜。抄經員交給你一張寫有『天上沒有回覆』的殘頁。" ); }, () => ColonyHas(ThingDefOf.Silver, 120)),
                new Dialog_ChapterOneEncounter.Choice("要求查驗契約", "利用公會見證拖延，迫使商路放棄這批無法證明來源的俘虜。", () => AdvanceChapter("公會代理人當眾逐條核對契約。押運官不願暴露上游買家，只能棄下俘虜撤離。你得到一張偽造神諭的抄本。")),
                new Dialog_ChapterOneEncounter.Choice("解開武器", "談判破裂，鎖鏈商路正規軍將進攻。", () => { TriggerRaid("Calido_SlaveCaravan"); AdvanceChapter("鎖鏈商路排成整齊隊列發動進攻。無論勝負，抄經員都把一張殘頁塞進留言柱縫隙。" ); })));
        }

        private void OpenCalidoClue()
        {
            Find.WindowStack.Add(new Dialog_ChapterOneEncounter(
                "卡利多線索：沉默不是神諭", "南渡記錄人",
                "殘頁上的文字不是祈禱，而是一次被刪改過的通訊紀錄：『地面節點已切斷。未經聽證，不得恢復授權。』落款被人刮去，只剩卡利多常用的環形記號。白羽聖堂稱它是試煉，蝕羽密教則宣稱沉默本身就是神諭。",
                new Dialog_ChapterOneEncounter.Choice("公開殘頁", "居民會知道天空主動切斷了聯絡。", () => AdvanceChapter("你把殘頁釘上留言柱。從今天起，南渡尋找的不是神蹟，而是誰下令切斷地面節點。")),
                new Dialog_ChapterOneEncounter.Choice("交給公會鑑定", "公會將建立獨立證物鏈。", () => AdvanceChapter("公會封存原件並留下拓本。這份證物將在第四章聽證會成為地面證詞。")),
                new Dialog_ChapterOneEncounter.Choice("暫時保密", "避免教會立刻爭奪，但密教會注意到你的沉默。", () => AdvanceChapter("只有留言冊最內頁記下真相。有人在夜裡試圖撬開留言柱，卻沒有找到原件。"))));
        }

        private void CompleteChapterOne()
        {
            chapterOneComplete = true;
            chapterOneWindowOpen = false;
            Find.LetterStack.ReceiveLetter("第一章完成：地面沒有神諭", "南渡站穩了腳步。冒險者公會承認這裡是委託點，強盜與鎖鏈商路暴露了共同標記，而卡利多的殘頁證明天空的沉默來自一道命令。\n\n第二章「斷翼商路」已解鎖。", LetterDefOf.PositiveEvent);
            campaignNextTick = (Find.TickManager?.TicksGame ?? 0) + 3000;
        }

        private void TickExtendedCampaign()
        {
            if (!chapterOneComplete || campaignBeat >= CalidoCampaignContent.Beats.Length || campaignWindowOpen || Find.TickManager == null || Find.AnyPlayerHomeMap == null) return;
            if (activeCampaignBeat >= 0)
            {
                if (Find.TickManager.TicksGame < activeCampaignResolveTick) return;
                if (activeCampaignBeat == 5 && CountPlayerBuildings("Calido_GroundSignalBeacon") < 3)
                {
                    activeCampaignResolveTick = Find.TickManager.TicksGame + 1200;
                    return;
                }
                if (activeCampaignCombat && Find.AnyPlayerHomeMap.mapPawns.AllPawnsSpawned.Any(p => p.HostileTo(Faction.OfPlayer) && !p.Downed))
                {
                    activeCampaignResolveTick = Find.TickManager.TicksGame + 600;
                    return;
                }
                CampaignBeat active = CalidoCampaignContent.Beats[activeCampaignBeat];
                ResolveCampaignBeat(active, activeCampaignHumane);
                activeCampaignBeat = -1;
                activeCampaignResolveTick = -1;
                activeCampaignCombat = false;
                return;
            }
            if (campaignNextTick < 0) campaignNextTick = Find.TickManager.TicksGame + 3000;
            if (Find.TickManager.TicksGame < campaignNextTick) return;
            CampaignBeat beat = CalidoCampaignContent.Beats[campaignBeat];
            campaignWindowOpen = true;
            Find.WindowStack.Add(new Dialog_ChapterOneEncounter(
                beat.Chapter + "：" + beat.Title, beat.HeroName + "・" + beat.HeroTitle,
                beat.Body + "\n\n完成此任務後，" + beat.HeroName + "會以「" + beat.HeroTitle + "」身分加入南渡。她只攜帶地面裝備。",
                new Dialog_ChapterOneEncounter.Choice("開始主要行動", "建立任務狀態；戰鬥任務必須清除來襲威脅才會完成。", () => StartCampaignBeat(beat, false)),
                new Dialog_ChapterOneEncounter.Choice("採取救援優先方案", "投入更多時間避免犧牲；完成後記錄為救援解法。", () => StartCampaignBeat(beat, true)),
                new Dialog_ChapterOneEncounter.Choice("延後 1 天", "不會失敗或跳過。", () => { campaignWindowOpen = false; campaignNextTick = Find.TickManager.TicksGame + 60000; })));
        }

        private void StartCampaignBeat(CampaignBeat beat, bool humane)
        {
            activeCampaignBeat = campaignBeat;
            activeCampaignHumane = humane;
            campaignWindowOpen = false;
            activeCampaignCombat = IsCombatBeat(campaignBeat);
            activeCampaignResolveTick = (Find.TickManager?.TicksGame ?? 0) + (humane ? 60000 : 30000);
            if (activeCampaignCombat)
            {
                string faction = campaignBeat <= 3 ? "Calido_SlaveCaravan" : campaignBeat >= 16 ? "Calido_FallenCult" : "Calido_BanditCompanies";
                TriggerRaid(faction);
                Find.LetterStack.ReceiveLetter("行動開始：" + beat.Title, "敵對部隊已被引出。守住南渡並清除仍有戰鬥能力的敵人後，任務才會結算。", LetterDefOf.ThreatBig);
            }
            else
            {
                Find.LetterStack.ReceiveLetter("行動開始：" + beat.Title, humane
                    ? "調查隊將花一整天核對證人、安置傷患並保存證物。期間不會立即跳過任務。"
                    : "調查、建設或聽證程序已開始；半天後將依目前成果結算。", LetterDefOf.NeutralEvent);
                if (campaignBeat == 5)
                    Find.LetterStack.ReceiveLetter("建設目標：三座地面航標塔", "從「雜項」建造三座地面航標塔。每座需要80鋼與2個零件；在三座都完工以前，任務不會自動結算。", LetterDefOf.PositiveEvent);
            }
        }

        private static int CountPlayerBuildings(string defName)
        {
            ThingDef def = DefDatabase<ThingDef>.GetNamedSilentFail(defName);
            return def == null ? 0 : Find.Maps.Where(m => m.IsPlayerHome).Sum(m => m.listerThings.ThingsOfDef(def).Count);
        }

        private static bool IsCombatBeat(int index)
        {
            return index == 0 || index == 2 || index == 3 || index == 4 || index == 6 || index == 15 || index == 16 || index == 18;
        }

        private void ResolveCampaignBeat(CampaignBeat beat, bool humane)
        {
            RecruitHero(beat);
            campaignBeat++;
            campaignWindowOpen = false;
            campaignNextTick = (Find.TickManager?.TicksGame ?? 0) + 3000;
            // Chapter five releases civilian, armour, weapon and Milian authority
            // in separate beats. Full authorization exists only after all four.
            calidoTechUnlocked = campaignBeat >= 16;
            if (campaignBeat >= CalidoCampaignContent.Beats.Length)
            {
                cultResurrectionUnlocked = true;
                GrantReturningLantern();
                Find.LetterStack.ReceiveLetter("主線完成：歸航不是歸順", "偽神諭的原稿已公開，地面證詞被卡利多正式承認。你可以往返天空與地面，高階科技授權已開放；白羽聖堂承認主角一直攜帶的渡口長槍正是神器的未覺醒形態。讓主角親自裝備一把渡口長槍，它將唯一一次進化為「歸燈」。邪教烙印則留下了一次以昏迷為代價的復生能力。", LetterDefOf.PositiveEvent);
            }
            else
            {
                Find.LetterStack.ReceiveLetter(beat.Title + "完成", (humane ? "你選擇了救援優先的處理方式。" : "主要目標已完成。") + "\n" + beat.HeroName + "加入殖民地。", LetterDefOf.NeutralEvent);
            }
        }

        private static void RecruitHero(CampaignBeat beat)
        {
            PawnKindDef kind = DefDatabase<PawnKindDef>.GetNamedSilentFail("Milira_Colonist");
            Map map = Find.AnyPlayerHomeMap;
            if (kind == null || map == null) return;
            Pawn pawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(kind, Faction.OfPlayer));
            pawn.Name = new NameTriple(beat.HeroName, beat.HeroTitle, "卡利多");
            ApplyHeroProfile(pawn, beat.HeroTitle);
            HediffDef specialty = DefDatabase<HediffDef>.GetNamedSilentFail(beat.Specialty);
            if (specialty != null) pawn.health.AddHediff(specialty);
            GenSpawn.Spawn(pawn, CellFinder.RandomClosewalkCellNear(map.Center, map, 10), map);
        }

        private static void ApplyHeroProfile(Pawn pawn, string title)
        {
            if (pawn?.skills == null) return;
            SkillDef primary = null;
            switch (title)
            {
                case "醫師": primary = SkillDefOf.Medicine; break;
                case "談判官": case "審計官": case "記錄官": primary = SkillDefOf.Social; break;
                case "獵手": case "射手": case "狙擊手": case "守望者": primary = SkillDefOf.Shooting; break;
                case "建築師": primary = SkillDefOf.Construction; break;
                case "近衛": case "劍士": case "盾衛": case "潛行者": primary = SkillDefOf.Melee; break;
                case "研究員": case "學者": primary = SkillDefOf.Intellectual; break;
                case "農藝師": primary = SkillDefOf.Plants; break;
                case "工匠": case "機械師": primary = SkillDefOf.Crafting; break;
                case "廚師": primary = SkillDefOf.Cooking; break;
            }
            if (primary != null)
            {
                SkillRecord skill = pawn.skills.GetSkill(primary);
                skill.Level = 20;
                skill.passion = Passion.Major;
            }
        }

        private void GrantReturningLantern()
        {
            if (returningLanternGranted) return;
            returningLanternEvolutionReady = true;
            TryEvolveReturningLantern();
            if (!returningLanternGranted && !returningLanternReminderSent)
            {
                returningLanternReminderSent = true;
                Find.LetterStack.ReceiveLetter("歸燈：等待主角喚醒", "渡口長槍已被證實是歸燈的未覺醒形態。請讓劇本主角親自裝備任意一把「渡口長槍」；只有她手中的那一把會被消耗並進化，其他可製作的長槍不受影響。", LetterDefOf.PositiveEvent);
            }
        }

        private void TryEvolveReturningLantern()
        {
            if (!returningLanternEvolutionReady || returningLanternGranted) return;
            Pawn protagonist = Find.Maps.SelectMany(m => m.mapPawns.FreeColonistsSpawned).FirstOrDefault(p => p.thingIDNumber == protagonistId);
            if (protagonist == null || !IsMilira(protagonist) || protagonist.equipment?.Primary?.def?.defName != "Calido_ReachSpear") return;
            ThingDef artifactDef = DefDatabase<ThingDef>.GetNamedSilentFail("Calido_ReturningLantern");
            if (artifactDef == null) return;
            protagonist.equipment.DestroyEquipment(protagonist.equipment.Primary);
            protagonist.equipment.AddEquipment((ThingWithComps)ThingMaker.MakeThing(artifactDef));
            returningLanternGranted = true;
            returningLanternEvolutionReady = false;
            Find.LetterStack.ReceiveLetter("歸燈覺醒", "主角手中的渡口長槍沒有被替換，而是在舊木柄與渡口銅印上長出了完整聖環、翼飾與青色核心。世界上只能存在這一把歸燈。", LetterDefOf.PositiveEvent, protagonist);
        }

        private void TickTechnologyLock()
        {
            if (Find.TickManager == null || Find.TickManager.TicksGame % 120 != 0) return;
            TryEvolveReturningLantern();
            foreach (Pawn pawn in Find.Maps.SelectMany(m => m.mapPawns.AllPawnsSpawned).Where(IsMilira))
            {
                ThingWithComps weapon = pawn.equipment?.Primary;
                if (!BlueprintFinished("Calido_Blueprint_Weapons") && weapon != null && IsLockedMiliraTech(weapon.def)) pawn.equipment.TryDropEquipment(weapon, out _, pawn.Position, forbid: false);
                UpdateReturningLanternAwakening(pawn, weapon);
                if (pawn.apparel != null)
                {
                    foreach (Apparel apparel in pawn.apparel.WornApparel.ToList())
                        if (!BlueprintFinished("Calido_Blueprint_Armor") && IsLockedMiliraTech(apparel.def)) pawn.apparel.TryDrop(apparel, out _, pawn.Position, forbid: false);
                }
            }

            // The relic is story-unique. This also cleans up copies created by
            // external reward pools or old development saves while keeping the
            // oldest existing instance.
            ThingDef lantern = DefDatabase<ThingDef>.GetNamedSilentFail("Calido_ReturningLantern");
            if (lantern != null)
            {
                List<Thing> copies = Find.Maps.SelectMany(m => m.listerThings.ThingsOfDef(lantern)).OrderBy(t => t.thingIDNumber).ToList();
                for (int i = 1; i < copies.Count; i++) copies[i].Destroy(DestroyMode.Vanish);
            }
        }

        private static void UpdateReturningLanternAwakening(Pawn pawn, ThingWithComps weapon)
        {
            HediffDef awakened = DefDatabase<HediffDef>.GetNamedSilentFail("Calido_ReturningLanternAwakened");
            if (awakened == null) return;
            bool holding = weapon?.def?.defName == "Calido_ReturningLantern";
            Hediff current = pawn.health.hediffSet.GetFirstHediffOfDef(awakened);
            if (holding && current == null) pawn.health.AddHediff(awakened);
            else if (!holding && current != null) pawn.health.RemoveHediff(current);
        }

        public static bool IsMilira(Pawn pawn) => pawn?.def?.defName?.IndexOf("Milira", System.StringComparison.OrdinalIgnoreCase) >= 0;
        public static bool IsLockedMiliraTech(ThingDef def)
        {
            if (def == null || def.defName == "Calido_ReturningLantern") return false;
            bool miliraNamed = def.defName.StartsWith("Milira_") || def.defName.StartsWith("Milian_");
            return miliraNamed && def.techLevel >= TechLevel.Spacer;
        }

        private void EnsureAuthorizationBlueprints()
        {
            if (Find.ResearchManager == null) return;
            if (campaignBeat >= 13) FinishAuthorizationBlueprint("Calido_Blueprint_Civilian", "已取得卡利多民用系統授權藍圖。");
            if (campaignBeat >= 14) FinishAuthorizationBlueprint("Calido_Blueprint_Armor", "已取得卡利多個人防具授權藍圖。");
            if (campaignBeat >= 15) FinishAuthorizationBlueprint("Calido_Blueprint_Weapons", "已取得卡利多武器系統授權藍圖。");
            if (campaignBeat >= 16) FinishAuthorizationBlueprint("Calido_Blueprint_Milian", "已取得米莉安指揮權授權藍圖。");
            calidoTechUnlocked = BlueprintFinished("Calido_Blueprint_Civilian") && BlueprintFinished("Calido_Blueprint_Armor") && BlueprintFinished("Calido_Blueprint_Weapons") &&
                (DefDatabase<ResearchProjectDef>.GetNamedSilentFail("Calido_Blueprint_Milian") == null || BlueprintFinished("Calido_Blueprint_Milian"));
        }

        private static void FinishAuthorizationBlueprint(string defName, string message)
        {
            ResearchProjectDef project = DefDatabase<ResearchProjectDef>.GetNamedSilentFail(defName);
            if (project == null || project.IsFinished) return;
            Find.ResearchManager.FinishProject(project, false, null, false);
            Messages.Message(message, MessageTypeDefOf.PositiveEvent, false);
        }

        private static bool BlueprintFinished(string defName)
        {
            ResearchProjectDef project = DefDatabase<ResearchProjectDef>.GetNamedSilentFail(defName);
            return project != null && project.IsFinished;
        }

        private void TickFallenResurrection()
        {
            if (!cultResurrectionUnlocked || Find.TickManager == null) return;
            Pawn protagonist = Find.Maps.SelectMany(m => m.mapPawns.FreeColonistsSpawned).FirstOrDefault(p => p.thingIDNumber == protagonistId);
            if (protagonist == null)
            {
                protagonist = Find.Maps.SelectMany(m => m.mapPawns.FreeColonistsSpawned).FirstOrDefault(IsMilira);
                if (protagonist != null) protagonistId = protagonist.thingIDNumber;
            }
            if (protagonist == null) return;
            HediffDef form = DefDatabase<HediffDef>.GetNamedSilentFail("Calido_FallenAngelForm");
            HediffDef exhaustionDef = DefDatabase<HediffDef>.GetNamedSilentFail("Calido_FallenExhaustion");
            bool exhausted = exhaustionDef != null && protagonist.health.hediffSet.HasHediff(exhaustionDef);
            if (!fallenFormActive && !exhausted && protagonist.Downed && form != null)
            {
                foreach (Hediff_MissingPart missing in protagonist.health.hediffSet.hediffs.OfType<Hediff_MissingPart>().ToList())
                    if (missing.Part != null) protagonist.health.RestorePart(missing.Part);
                foreach (Hediff h in protagonist.health.hediffSet.hediffs.Where(h => h.def.isBad).ToList()) protagonist.health.RemoveHediff(h);
                protagonist.health.AddHediff(form);
                fallenFormActive = true;
                fallenFormEndTick = Find.TickManager.TicksGame + 3600;
                fallenFormStartHealth = protagonist.health.summaryHealth.SummaryHealthPercent;
                Messages.Message("邪教烙印使主角滿血復生，墮天使形態將維持60秒。", protagonist, MessageTypeDefOf.ThreatBig);
            }
            if (fallenFormActive && Find.TickManager.TicksGame >= fallenFormEndTick)
            {
                if (form != null)
                {
                    Hediff active = protagonist.health.hediffSet.GetFirstHediffOfDef(form);
                    if (active != null) protagonist.health.RemoveHediff(active);
                }
                if (exhaustionDef != null)
                {
                    Hediff exhaustion = protagonist.health.AddHediff(exhaustionDef);
                    float healthLost = UnityEngine.Mathf.Clamp01(fallenFormStartHealth - protagonist.health.summaryHealth.SummaryHealthPercent);
                    int comaTicks = 60000 + UnityEngine.Mathf.RoundToInt(360000f * healthLost);
                    HediffComp_Disappears disappears = exhaustion.TryGetComp<HediffComp_Disappears>();
                    if (disappears != null) disappears.ticksToDisappear = comaTicks;
                    Messages.Message($"墮天使形態結束：健康損失 {healthLost:P0}，預計昏迷 {comaTicks / 60000f:0.0} 天。", protagonist, MessageTypeDefOf.NegativeHealthEvent);
                }
                fallenFormActive = false;
            }
        }

        private void TickCrossRaceEvents()
        {
            if (!messagePostQuestComplete || crossRaceEventWindowOpen || Find.TickManager == null || Find.AnyPlayerHomeMap == null) return;
            crossRaceEventsSeen ??= new List<int>();
            if (nextCrossRaceEventTick < 0) nextCrossRaceEventTick = Find.TickManager.TicksGame + 60000;
            if (Find.TickManager.TicksGame < nextCrossRaceEventTick) return;

            List<CrossRaceEventEntry> available = CalidoCrossRaceEvents.All.Where(e => RaceModActive(e.Race) && !crossRaceEventsSeen.Contains(e.Id)).ToList();
            if (available.Count == 0)
            {
                crossRaceEventsSeen.Clear();
                available = CalidoCrossRaceEvents.All.Where(e => RaceModActive(e.Race)).ToList();
            }
            if (available.Count == 0) { nextCrossRaceEventTick = Find.TickManager.TicksGame + 60000; return; }
            CrossRaceEventEntry entry = available.RandomElement();
            crossRaceEventWindowOpen = true;
            Find.WindowStack.Add(new Dialog_ChapterOneEncounter(
                entry.Title, entry.Speaker, entry.Body, "sheet:" + entry.PortraitSheet,
                new Dialog_ChapterOneEncounter.Choice("提供協助", "以救援與合作優先處理；對應種族陣營好感提高。", () => ResolveCrossRaceEvent(entry, "你提供了協助。這次相遇被記為南渡與" + entry.Race + "之間的善意往來。", 8)),
                new Dialog_ChapterOneEncounter.Choice("提出交換條件", "在公會見證下交換資源或情報；小幅提高好感。", () => ResolveCrossRaceEvent(entry, "雙方把條件寫進留言冊，在沒有神諭與口頭特權的情況下完成交換。", 3)),
                new Dialog_ChapterOneEncounter.Choice("保持距離", "拒絕介入但不主動敵對；小幅降低好感。", () => ResolveCrossRaceEvent(entry, "南渡拒絕介入。對方沒有立刻敵對，但在記錄中留下了保留意見。", -2))));
        }

        private void ResolveCrossRaceEvent(CrossRaceEventEntry entry, string outcome, int goodwill)
        {
            crossRaceEventsSeen ??= new List<int>();
            if (!crossRaceEventsSeen.Contains(entry.Id)) crossRaceEventsSeen.Add(entry.Id);
            crossRaceEventWindowOpen = false;
            nextCrossRaceEventTick = (Find.TickManager?.TicksGame ?? 0) + Rand.RangeInclusive(180000, 420000);
            Faction faction = FindRaceFaction(entry.Race);
            if (faction != null && faction != Faction.OfPlayer)
                faction.TryAffectGoodwillWith(Faction.OfPlayer, goodwill, false, false, null, null);
            Find.LetterStack.ReceiveLetter(entry.Title + "完成", outcome + $"\n\n跨種族事件進度：{crossRaceEventsSeen.Count}/160。", LetterDefOf.NeutralEvent);
        }

        private void TickGuildPrisonerBuyer()
        {
            if (!messagePostQuestComplete || guildPrisonerWindowOpen || Find.TickManager == null || Find.AnyPlayerHomeMap == null) return;
            if (nextGuildPrisonerVisitTick < 0) nextGuildPrisonerVisitTick = Find.TickManager.TicksGame + 420000;
            if (Find.TickManager.TicksGame < nextGuildPrisonerVisitTick) return;

            Pawn prisoner = Find.Maps.Where(m => m.IsPlayerHome)
                .SelectMany(m => m.mapPawns.AllPawnsSpawned)
                .Where(p => p.IsPrisonerOfColony && p.RaceProps.Humanlike && !p.Dead)
                .OrderByDescending(p => p.MarketValue)
                .FirstOrDefault();
            if (prisoner == null)
            {
                nextGuildPrisonerVisitTick = Find.TickManager.TicksGame + 120000;
                return;
            }

            int reward = UnityEngine.Mathf.Clamp(UnityEngine.Mathf.RoundToInt(prisoner.MarketValue * 0.35f), 120, 1200);
            guildPrisonerWindowOpen = true;
            Find.WindowStack.Add(new Dialog_ChapterOneEncounter(
                "冒險者公會：罪犯收押", "公會收押官",
                $"公會核對了殖民地的俘虜名冊，願意接收「{prisoner.LabelShortCap}」。這不是奴隸交易：公會將負責押送、審理與向受害地交代，殖民地可獲得 {reward} 銀的懸賞。",
                new Dialog_ChapterOneEncounter.Choice("交由公會收押", $"移交該俘虜並領取 {reward} 銀。", () => TransferPrisonerToGuild(prisoner, reward)),
                new Dialog_ChapterOneEncounter.Choice("要求日後再談", "保留俘虜；公會七天後再派人詢問。", () => { guildPrisonerWindowOpen = false; nextGuildPrisonerVisitTick = Find.TickManager.TicksGame + 420000; }),
                new Dialog_ChapterOneEncounter.Choice("拒絕本次收押", "不移交；三天後可處理其他俘虜。", () => { guildPrisonerWindowOpen = false; nextGuildPrisonerVisitTick = Find.TickManager.TicksGame + 180000; })));
        }

        private void TransferPrisonerToGuild(Pawn prisoner, int reward)
        {
            guildPrisonerWindowOpen = false;
            nextGuildPrisonerVisitTick = (Find.TickManager?.TicksGame ?? 0) + 420000;
            if (prisoner == null || prisoner.DestroyedOrNull() || !prisoner.IsPrisonerOfColony) return;
            Map map = prisoner.Map;
            IntVec3 pos = prisoner.Position;
            Faction guild = Find.FactionManager?.AllFactions.FirstOrDefault(f => f.def.defName == "Calido_AdventurersGuild");
            if (prisoner.Spawned) prisoner.DeSpawn();
            if (guild != null) prisoner.SetFaction(guild);
            Find.WorldPawns.PassToWorld(prisoner, PawnDiscardDecideMode.KeepForever);
            Thing silver = ThingMaker.MakeThing(ThingDefOf.Silver);
            silver.stackCount = reward;
            GenPlace.TryPlaceThing(silver, pos.IsValid ? pos : map.Center, map, ThingPlaceMode.Near);
            Find.LetterStack.ReceiveLetter("公會完成收押", $"公會押送隊接管了俘虜並支付 {reward} 銀。此人不再屬於殖民地，也不會被當成奴隸出售。", LetterDefOf.PositiveEvent);
        }

        private static bool RaceModActive(string race)
        {
            string packageId = race == "美狐" ? "miho.fortifiedoutremer" : race == "奇羅" ? "ancot.kiirorace" : race == "沃芬" ? "melondove.wolfeinrace" : "hentailoliteam.axolotl";
            return LoadedModManager.RunningModsListForReading.Any(m => m.PackageIdPlayerFacing.Equals(packageId, System.StringComparison.OrdinalIgnoreCase));
        }

        private static Faction FindRaceFaction(string race)
        {
            string[] defs = race == "美狐" ? new[] { "Miho_Faction_Megacorp", "Miho_Faction_Supremacist" }
                : race == "奇羅" ? new[] { "Kiiro_Faction" }
                : race == "沃芬" ? new[] { "Wolfein_Faction_MegaCorp", "Wolfein_Faction_Rebel" }
                : new[] { "Axolotl_Faction", "Axolotl_BloodSect" };
            return Find.FactionManager?.AllFactions.FirstOrDefault(f => defs.Contains(f.def.defName));
        }

        private void AdvanceChapter(string outcome)
        {
            chapterOneWindowOpen = false;
            chapterOneStep++;
            chapterOneNextTick = (Find.TickManager?.TicksGame ?? 0) + 1800;
            Find.LetterStack.ReceiveLetter("南渡記事更新", outcome, LetterDefOf.NeutralEvent);
        }

        private static bool ColonyHas(ThingDef def, int count)
        {
            return Find.Maps.Where(m => m.IsPlayerHome).SelectMany(m => m.listerThings.ThingsOfDef(def)).Where(t => t.Faction == Faction.OfPlayer || t.IsInAnyStorage()).Sum(t => t.stackCount) >= count;
        }

        private static void ConsumeFromColony(ThingDef def, int count)
        {
            foreach (Thing thing in Find.Maps.Where(m => m.IsPlayerHome).SelectMany(m => m.listerThings.ThingsOfDef(def)).ToList())
            {
                int take = System.Math.Min(count, thing.stackCount);
                thing.SplitOff(take).Destroy();
                count -= take;
                if (count <= 0) return;
            }
        }

        private static void TriggerRaid(string factionDefName)
        {
            Map map = Find.AnyPlayerHomeMap;
            Faction faction = Find.FactionManager.AllFactions.FirstOrDefault(f => f.def.defName == factionDefName);
            if (map == null || faction == null) return;
            IncidentParms parms = StorytellerUtility.DefaultParmsNow(IncidentCategoryDefOf.ThreatBig, map);
            parms.faction = faction;
            parms.points = StorytellerUtility.DefaultThreatPointsNow(map);
            IncidentDefOf.RaidEnemy.Worker.TryExecute(parms);
        }

        public void CompletePrologue(CalidoPrologueChoice choice)
        {
            prologueChoice = choice;
            prologueComplete = true;
            prologueQueued = false;
            featherEventStartTick = (Find.TickManager?.TicksGame ?? 0) + 1200;

            Find.LetterStack.ReceiveLetter(
                "南渡留言冊已開啟",
                "渡口的人替你留下一本沒有寫滿的留言冊。從畫面下方的「南渡留言冊」可以重新閱讀序幕，並查看之後出現的地面委託。",
                LetterDefOf.NeutralEvent);
        }

        public void CompleteFeatherEvent(CalidoFeatherChoice choice)
        {
            featherChoice = choice;
            featherEventComplete = true;
            featherEventQueued = false;
            messagePostQuestStartTick = (Find.TickManager?.TicksGame ?? 0) + 600;

            Find.LetterStack.ReceiveLetter(
                "井邊的羽毛",
                FeatherOutcomeText(choice) + "\n\n這件事已被寫進南渡留言冊。新的建設請託將在居民確認你的態度後出現。",
                LetterDefOf.NeutralEvent);
        }

        public void ReopenFeatherEvent()
        {
            if (Find.WindowStack != null)
            {
                Find.WindowStack.Add(new Dialog_FeatherAtWell(this, true));
            }
        }

        public static string FeatherOutcomeText(CalidoFeatherChoice choice)
        {
            switch (choice)
            {
                case CalidoFeatherChoice.ReturnIt:
                    return "你讓孩子把羽毛交回本人。村民第一次明白，米莉拉身上的東西不是自動屬於祭壇。";
                case CalidoFeatherChoice.KeepAsCommonMemory:
                    return "你允許孩子保留羽毛，卻要求它只作為相遇的紀念。有人仍稱它神蹟，也有人開始稱它故事。";
                case CalidoFeatherChoice.ForbidWorship:
                    return "你明確禁止供奉與販售羽毛。祭司不滿，但市場上的假羽商人暫時收起了招牌。";
                default:
                    return "井邊的爭論尚未有結果。";
            }
        }

        public void ReopenPrologue()
        {
            if (Find.WindowStack != null)
            {
                Find.WindowStack.Add(new Dialog_BorrowedName(this, true));
            }
        }

        public override void ExposeData()
        {
            Scribe_Values.Look(ref scenarioEnabled, "scenarioEnabled", false);
            Scribe_Values.Look(ref prologueQueued, "prologueQueued", false);
            Scribe_Values.Look(ref prologueComplete, "prologueComplete", false);
            Scribe_Values.Look(ref prologueStartTick, "prologueStartTick", -1);
            Scribe_Values.Look(ref prologueChoice, "prologueChoice", CalidoPrologueChoice.None);
            Scribe_Values.Look(ref featherEventQueued, "featherEventQueued", false);
            Scribe_Values.Look(ref featherEventComplete, "featherEventComplete", false);
            Scribe_Values.Look(ref featherEventStartTick, "featherEventStartTick", -1);
            Scribe_Values.Look(ref featherChoice, "featherChoice", CalidoFeatherChoice.None);
            Scribe_Values.Look(ref messagePostQuestAnnounced, "messagePostQuestAnnounced", false);
            Scribe_Values.Look(ref messagePostQuestComplete, "messagePostQuestComplete", false);
            Scribe_Values.Look(ref messagePostQuestStartTick, "messagePostQuestStartTick", -1);
            Scribe_Values.Look(ref groundWorldSanitized, "groundWorldSanitized", false);
            Scribe_Values.Look(ref miliraDiplomacyInitialized, "miliraDiplomacyInitialized", false);
            Scribe_Values.Look(ref factionDiplomacyInitialized, "factionDiplomacyInitialized", false);
            Scribe_Values.Look(ref chapterOneStep, "chapterOneStep", 0);
            Scribe_Values.Look(ref chapterOneNextTick, "chapterOneNextTick", -1);
            Scribe_Values.Look(ref firstHeroRecruited, "firstHeroRecruited", false);
            Scribe_Values.Look(ref chapterOneComplete, "chapterOneComplete", false);
            Scribe_Values.Look(ref campaignBeat, "campaignBeat", 0);
            Scribe_Values.Look(ref campaignNextTick, "campaignNextTick", -1);
            Scribe_Values.Look(ref activeCampaignBeat, "activeCampaignBeat", -1);
            Scribe_Values.Look(ref activeCampaignResolveTick, "activeCampaignResolveTick", -1);
            Scribe_Values.Look(ref activeCampaignHumane, "activeCampaignHumane", false);
            Scribe_Values.Look(ref activeCampaignCombat, "activeCampaignCombat", false);
            Scribe_Values.Look(ref calidoTechUnlocked, "calidoTechUnlocked", false);
            Scribe_Values.Look(ref cultResurrectionUnlocked, "cultResurrectionUnlocked", false);
            Scribe_Values.Look(ref returningLanternGranted, "returningLanternGranted", false);
            Scribe_Values.Look(ref returningLanternEvolutionReady, "returningLanternEvolutionReady", false);
            Scribe_Values.Look(ref returningLanternReminderSent, "returningLanternReminderSent", false);
            Scribe_Values.Look(ref protagonistId, "protagonistId", -1);
            Scribe_Values.Look(ref fallenFormActive, "fallenFormActive", false);
            Scribe_Values.Look(ref fallenFormEndTick, "fallenFormEndTick", -1);
            Scribe_Values.Look(ref fallenFormStartHealth, "fallenFormStartHealth", 1f);
            Scribe_Collections.Look(ref crossRaceEventsSeen, "crossRaceEventsSeen", LookMode.Value);
            Scribe_Values.Look(ref nextCrossRaceEventTick, "nextCrossRaceEventTick", -1);
            Scribe_Values.Look(ref nextGuildPrisonerVisitTick, "nextGuildPrisonerVisitTick", -1);
            Scribe_Values.Look(ref cultActivated, "cultActivated", false);
            Scribe_Values.Look(ref cultDiplomacyInitialized, "cultDiplomacyInitialized", false);
            Scribe_Values.Look(ref startingReachSpearGranted, "startingReachSpearGranted", false);

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                // Windows are not saved. A queued-but-unfinished scene must be allowed
                // to open again after loading instead of becoming permanently stuck.
                prologueQueued = false;
                featherEventQueued = false;
                chapterOneWindowOpen = false;
                campaignWindowOpen = false;
                crossRaceEventWindowOpen = false;
                guildPrisonerWindowOpen = false;
                crossRaceEventsSeen ??= new List<int>();
            }
        }
    }
}
