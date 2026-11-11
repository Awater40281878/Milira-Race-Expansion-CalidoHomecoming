using RimWorld;
using System.Linq;
using Verse;

namespace CalidoHomecoming
{
    public sealed class Building_SlaveIronCage : Building_Casket
    {
        private string originFactionDefName;

        public void SetOriginFaction(Faction faction)
        {
            originFactionDefName = faction?.def?.defName;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref originFactionDefName, "originFactionDefName");
        }

        public override void Open()
        {
            Pawn captive = ContainedThing as Pawn;
            base.Open();
            if (captive == null || captive.DestroyedOrNull()) return;

            Hediff sickness = captive.health?.hediffSet?.GetFirstHediffOfDef(HediffDefOf.CryptosleepSickness);
            if (sickness != null) captive.health.RemoveHediff(sickness);
            ApplyCaptivityInjuries(captive);
            Find.WindowStack.Add(new Dialog_ChapterOneEncounter(
                "鐵籠裡的人", captive.LabelShortCap,
                "你撬開鐵鎖。對方沒有休眠副作用，但長期押運留下的傷勢仍清晰可見。\n\n健康評估：" + HealthGrade(captive) + "\n來源：" + (OriginFaction()?.Name ?? "無法確認") + "\n\n「我不想再被任何人標價。」",
                new Dialog_ChapterOneEncounter.Choice("收留", "讓她加入南渡。", () => Recruit(captive)),
                new Dialog_ChapterOneEncounter.Choice("放她離開", "提供自由；她會自行離開地圖。", () => Release(captive)),
                new Dialog_ChapterOneEncounter.Choice("資助返程物資", "給予 80 銀的路費並改善其母陣營關係。", () => Aid(captive), () => ColonySilver() >= 80)));
        }

        public override string GetInspectString()
        {
            string text = base.GetInspectString();
            return HasAnyContents ? text + "\n？籠內仍有人。" : text;
        }

        private static void ApplyCaptivityInjuries(Pawn pawn)
        {
            var legs = pawn.RaceProps.body.AllParts.Where(p => p.def.tags != null && p.def.tags.Any(t => t.defName.Contains("MovingLimb"))).ToList();
            if (Rand.Value < 0.60f && legs.Any()) pawn.TakeDamage(new DamageInfo(DamageDefOf.Cut, Rand.RangeInclusive(5, 12), hitPart: legs.RandomElement()));
            if (Rand.Value < 0.60f && legs.Any()) pawn.TakeDamage(new DamageInfo(DamageDefOf.Cut, Rand.RangeInclusive(5, 12), hitPart: legs.RandomElement()));
            BodyPartRecord torso = pawn.RaceProps.body.AllParts.FirstOrDefault(p => p.def.defName == "Torso");
            if (Rand.Value < 0.10f && torso != null) pawn.TakeDamage(new DamageInfo(DamageDefOf.Bullet, Rand.RangeInclusive(14, 22), hitPart: torso));
        }

        private static string HealthGrade(Pawn pawn)
        {
            float health = pawn.health?.summaryHealth?.SummaryHealthPercent ?? 1f;
            if (health < 0.45f) return "嚴重";
            if (health < 0.70f) return "中等";
            if (health < 0.90f) return "輕";
            return "正常";
        }

        private void Recruit(Pawn pawn)
        {
            pawn.SetFaction(Faction.OfPlayer);
            Current.Game?.GetComponent<CalidoStoryState>()?.NotifySlaveCageOutcome("收留", OriginFaction(), pawn.Map);
            Messages.Message(pawn.LabelShortCap + "加入了南渡。", pawn, MessageTypeDefOf.PositiveEvent, false);
        }

        private void Release(Pawn pawn)
        {
            pawn.SetFaction(null);
            pawn.mindState.exitMapAfterTick = Find.TickManager.TicksGame + 180;
            Current.Game?.GetComponent<CalidoStoryState>()?.NotifySlaveCageOutcome("釋放", OriginFaction(), pawn.Map);
            Messages.Message(pawn.LabelShortCap + "帶著自由離開了地圖。", pawn, MessageTypeDefOf.NeutralEvent, false);
        }

        private void Aid(Pawn pawn)
        {
            int remaining = 80;
            foreach (Thing silver in Find.CurrentMap.listerThings.ThingsOfDef(ThingDefOf.Silver))
            {
                int take = System.Math.Min(remaining, silver.stackCount);
                silver.SplitOff(take).Destroy(); remaining -= take;
                if (remaining <= 0) break;
            }
            Faction origin = OriginFaction();
            pawn.SetFaction(null);
            pawn.mindState.exitMapAfterTick = Find.TickManager.TicksGame + 180;
            Current.Game?.GetComponent<CalidoStoryState>()?.NotifySlaveCageOutcome("資助", origin, pawn.Map);
            Messages.Message(pawn.LabelShortCap + "帶著路費與自由離開了地圖。", pawn, MessageTypeDefOf.PositiveEvent, false);
        }

        private static int ColonySilver() => Find.CurrentMap?.listerThings.ThingsOfDef(ThingDefOf.Silver).Sum(t => t.stackCount) ?? 0;

        private Faction OriginFaction()
        {
            return string.IsNullOrEmpty(originFactionDefName)
                ? null
                : Find.FactionManager.AllFactions.FirstOrDefault(f => f.def.defName == originFactionDefName);
        }
    }
}
