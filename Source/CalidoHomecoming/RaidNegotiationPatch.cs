using HarmonyLib;
using RimWorld;
using System;
using System.Linq;
using Verse;

namespace CalidoHomecoming
{
    [HarmonyPatch(typeof(IncidentWorker_RaidEnemy), "TryExecuteWorker")]
    public static class RaidNegotiationPatch
    {
        private static bool bypassOnce;
        private static int peaceUntilTick;

        public static bool Prefix(IncidentWorker_RaidEnemy __instance, IncidentParms parms, ref bool __result)
        {
            if (bypassOnce) { bypassOnce = false; return true; }
            CalidoStoryState state = Current.Game?.GetComponent<CalidoStoryState>();
            if (state?.ScenarioEnabled != true || parms?.target is not Map map || parms.faction == null) return true;
            if (IsNonNegotiating(parms.faction)) return true;
            int now = Find.TickManager?.TicksGame ?? 0;
            if (now < peaceUntilTick) { __result = false; return false; }

            string defName = parms.faction.def.defName;
            bool bandits = defName == "Calido_BanditCompanies" || parms.faction.def == FactionDefOf.Pirate;
            bool slavers = defName == "Calido_SlaveCaravan";
            int demand = Math.Max(bandits ? 180 : 80, (int)(parms.points * (bandits ? 0.34f : 0.18f)));
            string factionName = parms.faction.Name;
            Pawn transferable = slavers ? Find.Maps.Where(m => m.IsPlayerHome).SelectMany(m => m.mapPawns.AllPawnsSpawned).FirstOrDefault(p => p.IsPrisonerOfColony && p.RaceProps.Humanlike) : null;
            string demandText = bandits
                ? $"他們要求 {demand} 銀、藥物與自由出入倉庫的權利。這是一份刻意不合理的條件。"
                : slavers
                    ? $"他們要求支付 {demand} 銀，或交出一名殖民地囚犯作為『契約抵押』。"
                    : $"他們要求支付 {demand} 銀，否則將按原計畫進攻。";
            Find.WindowStack.Add(new Dialog_ChapterOneEncounter(
                "襲擊前交涉：" + factionName, factionName + "・前線使者",
                "武裝隊伍停在聚落射程之外，先派使者提出條件。" + demandText + "無論是否接受，答覆都會被記進南渡留言冊。",
                new Dialog_ChapterOneEncounter.Choice("支付 " + demand + " 銀", "支付後本次軍隊撤退，三天內不會再次提出襲擊交涉。", () => { ConsumeSilver(demand); peaceUntilTick = now + 180000; }, () => ColonySilver() >= demand),
                new Dialog_ChapterOneEncounter.Choice(slavers ? "交出一名囚犯" : "提出物資交換", slavers ? "把殖民地囚犯交給鎖鏈商路；這會避免本次戰鬥，但屬於有明確代價的選擇。" : "以糧食、道路情報與公會見證換取撤退；本次不扣銀。", () => { if (slavers) TransferPrisoner(transferable, parms.faction); peaceUntilTick = now + 120000; parms.faction.TryAffectGoodwillWith(Faction.OfPlayer, 1, false, false, null, null); }, () => !slavers || transferable != null),
                new Dialog_ChapterOneEncounter.Choice("拒絕條件", "交涉結束，對方立即按原襲擊方案進攻。", () => { bypassOnce = true; __instance.TryExecute(parms); })));
            __result = false;
            return false;
        }

        private static bool IsNonNegotiating(Faction faction)
        {
            string name = faction.def.defName.ToLowerInvariant();
            return name.Contains("mech") || name.Contains("insect") || name.Contains("entity") || faction == Faction.OfMechanoids || faction == Faction.OfInsects;
        }

        private static int ColonySilver() => Find.Maps.Where(m => m.IsPlayerHome).SelectMany(m => m.listerThings.ThingsOfDef(ThingDefOf.Silver)).Sum(t => t.stackCount);

        private static void ConsumeSilver(int amount)
        {
            foreach (Thing silver in Find.Maps.Where(m => m.IsPlayerHome).SelectMany(m => m.listerThings.ThingsOfDef(ThingDefOf.Silver)).ToList())
            {
                int take = Math.Min(amount, silver.stackCount);
                silver.SplitOff(take).Destroy();
                amount -= take;
                if (amount <= 0) break;
            }
        }

        private static void TransferPrisoner(Pawn pawn, Faction faction)
        {
            if (pawn == null || pawn.DestroyedOrNull() || !pawn.IsPrisonerOfColony) return;
            if (pawn.Spawned) pawn.DeSpawn();
            pawn.SetFaction(faction);
            Find.WorldPawns.PassToWorld(pawn, RimWorld.Planet.PawnDiscardDecideMode.KeepForever);
        }
    }
}
