using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace CalidoHomecoming
{
    public sealed class CalidoHeroRegistry : GameComponent
    {
        private List<Pawn> heroes = new List<Pawn>();
        private List<int> reportedLosses = new List<int>();
        private int nextCreateTick;

        public CalidoHeroRegistry(Game game) { }

        public override void GameComponentTick()
        {
            var state = Current.Game?.GetComponent<CalidoStoryState>();
            if (state?.ScenarioEnabled != true || Find.WorldPawns == null || Find.TickManager.TicksGame < nextCreateTick) return;
            nextCreateTick = Find.TickManager.TicksGame + 2500;
            heroes ??= new List<Pawn>();
            reportedLosses ??= new List<int>();
            if (heroes.Count < 20)
            {
                EnsureHero(heroes.Count);
            }
            for (int i = 0; i < heroes.Count; i++)
            {
                Pawn hero = heroes[i];
                if (hero == null || reportedLosses.Contains(i)) continue;
                bool lost = hero.Dead || (hero.Faction != Faction.OfPlayer && HasJoined(i));
                if (!lost) continue;
                reportedLosses.Add(i);
                state.NotifyHeroLost(HeroName(i), hero.Dead);
            }
        }

        private List<int> joined = new List<int>();
        public bool HasJoined(int index) => joined != null && joined.Contains(index);
        public Pawn FindHero(int index) => heroes != null && index >= 0 && index < heroes.Count ? heroes[index] : null;
        public Pawn EnsureHero(int index)
        {
            heroes ??= new List<Pawn>();
            if (index < 0 || index >= 20) return null;
            PawnKindDef kind = DefDatabase<PawnKindDef>.GetNamedSilentFail("Milira_Colonist");
            if (kind == null || Find.WorldPawns == null) return null;
            while (heroes.Count <= index)
            {
                int next = heroes.Count;
                string name = HeroName(next);
                Pawn existing = Find.WorldPawns.AllPawnsAliveOrDead.Concat(Find.Maps.SelectMany(m => m.mapPawns.AllPawns))
                    .FirstOrDefault(p => p.Name is NameTriple triple && triple.First == name);
                if (existing == null)
                {
                    existing = PawnGenerator.GeneratePawn(new PawnGenerationRequest(kind, null));
                    string title = next == 0 ? "遠棧" : CalidoCampaignContent.Beats[next - 1].HeroTitle;
                    existing.Name = new NameTriple(name, name, title);
                    Find.WorldPawns.PassToWorld(existing, PawnDiscardDecideMode.KeepForever);
                }
                heroes.Add(existing);
            }
            return heroes[index];
        }
        public void MarkJoined(int index)
        {
            joined ??= new List<int>();
            if (!joined.Contains(index)) joined.Add(index);
        }
        public static string HeroName(int index) => index == 0 ? "伊蕾希雅" : CalidoCampaignContent.Beats[index - 1].HeroName;
        public int IndexOf(string name)
        {
            for (int i = 0; i < 20; i++) if (HeroName(i) == name) return i;
            return -1;
        }
        public override void ExposeData()
        {
            Scribe_Collections.Look(ref heroes, "calidoHeroes", LookMode.Reference);
            Scribe_Collections.Look(ref joined, "calidoHeroesJoined", LookMode.Value);
            Scribe_Collections.Look(ref reportedLosses, "calidoHeroLosses", LookMode.Value);
            Scribe_Values.Look(ref nextCreateTick, "calidoNextHeroCreateTick");
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                heroes ??= new List<Pawn>();
                joined ??= new List<int>();
                reportedLosses ??= new List<int>();
            }
        }
    }
}
