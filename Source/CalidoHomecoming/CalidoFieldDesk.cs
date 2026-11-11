using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.AI.Group;

namespace CalidoHomecoming
{
    public sealed class CalidoSlaverMap : MapComponent
    {
        public CalidoSlaverMap(Map map) : base(map) {}
        public override void MapGenerated()
        {
            int cages = Rand.RangeInclusive(1, 3);
            if (Current.Game.GetComponent<CalidoStoryState>().ScenarioEnabled && map.Parent is Settlement &&
                !(map.Parent is CalidoMissionSite) && map.ParentFaction?.def.defName == "Calido_SlaveCaravan")
                for (int i = 0; i < cages; i++)
                    CalidoStoryState.SpawnSlaveCage(map, map.ParentFaction, map.Center);
        }
    }

    public sealed class CalidoFieldDesk : Building
    {
        private int stage;
        private Pawn guardian;
        private string cover;
        private bool submitted;
        private static readonly string[] Kinds = { "Milian_Mechanoid_KnightIV", "Milian_Mechanoid_BishopIV", "Milian_Mechanoid_RookIV" };
        private static readonly string[] Names = { "無訊", "斷鏈", "靜潮" };
        private static readonly string[] Salvage = { "Calido_SilentCoil", "Calido_BrokenLink", "Calido_TideFilter" };
        public override void TickRare()
        {
            base.TickRare();
            if (guardian?.Spawned != true || guardian.Downed || guardian.Map != Map) return;
            if (stage == 0)
                GenExplosion.DoExplosion(guardian.Position, Map, 2.4f, DamageDefOf.Smoke, guardian);
            else if (stage == 1)
            {
                var target = Map.mapPawns.FreeColonistsSpawned.FirstOrDefault(p => !p.Downed && p.Position.DistanceTo(guardian.Position) <= 7f);
                target?.stances?.stunner?.StunFor(55, guardian);
            }
            else if (stage == 2)
            {
                foreach (Pawn p in Map.mapPawns.FreeColonistsSpawned.Where(p => p.Position.DistanceTo(guardian.Position) <= 4f).ToList())
                    HealthUtility.AdjustSeverity(p, HediffDefOf.ToxicBuildup, 0.025f);
            }
        }
        internal static void Place(Map map, IntVec3 near)
        {
            GenPlace.TryPlaceThing(ThingMaker.MakeThing(DefDatabase<ThingDef>.GetNamed("Calido_FieldDesk")), near, map, ThingPlaceMode.Near);
        }
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref stage, "stage");
            Scribe_Values.Look(ref submitted, "submitted");
            Scribe_Values.Look(ref cover, "cover");
            Scribe_References.Look(ref guardian, "guardian");
        }
        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (var g in base.GetGizmos()) yield return g;
            var site = Map.Parent as CalidoMissionSite;
            var state = Current.Game.GetComponent<CalidoStoryState>();
            if (!state.IsCurrentSite(site) || submitted) yield break;
            bool machine = site.campaignBeat == 15;
            var cmd = new Command_Action();
            cmd.defaultLabel = machine ? (guardian == null ? "接通信標・" : "拆取殘件・") + Names[stage] :
                cover == null ? "選擇潛入身分" : stage == 0 ? "核對抄寫底稿" : "向抄經員出示原稿";
            cmd.defaultDesc = "讓一名清醒隊員走到調查台兩格內。取得證物後帶回基地並卸貨。";
            if (site.campaignBeat == 17) cmd.defaultLabel = "比對原稿與塗改筆跡";
            if (site.campaignBeat == 18) cmd.defaultLabel = "開啟契約・拒絕獻祭";
            if (!Map.mapPawns.FreeColonistsSpawned.Any(p => !p.Downed && p.Position.DistanceTo(Position) <= 2f))
                cmd.Disable("需要一名隊員在調查台旁");
            else if (machine && guardian != null && !guardian.Dead) cmd.Disable("先擊敗已召喚的守衛");
            ThingDef offering = machine ? new[]{ ThingDefOf.Steel, ThingDefOf.ComponentIndustrial, ThingDefOf.MedicineIndustrial }[stage] : null;
            int price = stage == 0 ? 10 : 1;
            Pawn operatorPawn = Map.mapPawns.FreeColonistsSpawned.FirstOrDefault(p => !p.Downed && p.Position.DistanceTo(Position) <= 2f);
            if (machine && guardian == null)
            {
                cmd.defaultDesc += $" 召喚需攜帶 {price} × {offering.LabelCap}，放在台旁亦可。";
                if (Payment(operatorPawn, offering, price) == null) cmd.Disable(cmd.defaultDesc);
            }
            cmd.action = () =>
            {
                if (machine)
                {
                    if (guardian == null)
                    {
                        var kind = DefDatabase<PawnKindDef>.GetNamed(Kinds[stage]);
                        Thing payment = Payment(operatorPawn, offering, price);
                        if (payment == null) return;
                        guardian = PawnGenerator.GeneratePawn(kind, Faction.OfMechanoids);
                        payment.SplitOff(price).Destroy();
                        guardian.Name = new NameSingle(Names[stage]);
                        GenSpawn.Spawn(guardian, CellFinder.RandomClosewalkCellNear(Position, Map, 9), Map);
                        LordMaker.MakeNewLord(Faction.OfMechanoids, new LordJob_DefendPoint(Position), Map, new[]{guardian});
                        return;
                    }
                    var reward = ThingMaker.MakeThing(DefDatabase<ThingDef>.GetNamed(Salvage[stage]));
                    reward.stackCount = 1;
                    GenPlace.TryPlaceThing(reward, Position, Map, ThingPlaceMode.Near);
                    guardian = null;
                    if (++stage < 3) return;
                }
                else if (site.campaignBeat == 17 || site.campaignBeat == 18)
                {
                    bool oath = site.campaignBeat == 18;
                    Find.WindowStack.Add(new Dialog_ChapterOneEncounter(oath ? "祭壇上的空位" : "兩份原稿", oath ? "契約守望者" : "值夜抄經員",
                        oath ? "「祭品的名字留白了。」契約末行另有一種寫法：由持印者承受代價。帶回烙印後，這條誓約才會生效。" : "「第一份有日期，第二份有主祭的印。少了任何一頁，他都能說是我自作主張。」",
                        new Dialog_ChapterOneEncounter.Choice(oath ? "留下自己的印記" : "同時封存原稿與改稿", "把原件帶回南渡交驗。", () => Complete(state)),
                        new Dialog_ChapterOneEncounter.Choice("暫時退開", "保留現場，稍後再決定。", () => {})));
                    return;
                }
                else if (cover == null)
                {
                    var options = new List<FloatMenuOption>();
                    foreach (string role in new[]{"送餐者", "抄寫員", "被捕信徒"})
                        options.Add(new FloatMenuOption(role, () => cover = role));
                    Find.WindowStack.Add(new FloatMenu(options));
                    return;
                }
                else if (stage++ == 0)
                {
                    string clue = cover == "送餐者" ? "餐單上的人數比登記囚徒多一人。你循送餐記號找到了藏匿的抄經員。" : cover == "抄寫員" ? "你比對兩種墨色，發現戒律的末句出自另一隻手。" : "隔壁囚室傳來背誦聲。囚徒記得刪改前的原文，並說出了抄經員的姓名。";
                    Find.LetterStack.ReceiveLetter("夾頁中的命令", clue + " 再次操作調查台，請抄經員對質。", LetterDefOf.NeutralEvent, this);
                    return;
                }
                if (!machine)
                {
                    Find.WindowStack.Add(new Dialog_ChapterOneEncounter("抄經所・當面對質", "值夜抄經員",
                        "「天上的原話是『暫停通訊』。『地上的人不配被聽見』是後來加上的。你若帶走原稿，他們會知道是我。」",
                        new Dialog_ChapterOneEncounter.Choice("匿名封存證詞", "帶走原稿並保護證人。", () => { state.RecordWitness(true); Complete(state); }),
                        new Dialog_ChapterOneEncounter.Choice("署名公開指控", "取得具名證詞，承擔報復風險。", () => { state.RecordWitness(false); Complete(state); })));
                    return;
                }
                Complete(state);
            };
            yield return cmd;
        }

        private Thing Payment(Pawn pawn, ThingDef def, int count)
        {
            return pawn?.inventory?.innerContainer.FirstOrDefault(t => t.def == def && t.stackCount >= count)
                ?? Map.listerThings.ThingsOfDef(def).FirstOrDefault(t => t.stackCount >= count && t.Position.DistanceTo(Position) <= 2f);
        }

        private void Complete(CalidoStoryState state)
        {
                if (submitted) return;
                submitted = true;
                state.PlaceMissionEvidence(Map, Position);
                Messages.Message("原始證物已放在調查台旁，請帶回南渡。", this, MessageTypeDefOf.PositiveEvent);
        }
    }
}
