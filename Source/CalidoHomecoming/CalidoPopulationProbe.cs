using System;
using System.Linq;
using RimWorld;
using Verse;

namespace CalidoHomecoming
{
    // Opt-in integration check for a disposable quicktest world.
    public sealed class CalidoPopulationProbe : GameComponent
    {
        private bool pending = Environment.GetCommandLineArgs().Contains("-calidoPopulationProbe");
        public CalidoPopulationProbe(Game game) {}
        public override void GameComponentUpdate()
        {
            if (!pending || Find.AnyPlayerHomeMap == null || LongEventHandler.AnyEventNowOrWaiting) return;
            pending = false;
            foreach (var name in new[]{"Calido_MihoKingdom", "Calido_KiiroKingdom", "Calido_WolfeinKingdom", "Calido_AxolotlRiverfolk"})
            {
                try
                {
                    var def = DefDatabase<FactionDef>.GetNamed(name);
                    var f = Find.FactionManager.AllFactions.FirstOrDefault(x => x.def == def);
                    if (f == null)
                    {
                        f = FactionGenerator.NewGeneratedFaction(new FactionGeneratorParms(def));
                        Find.FactionManager.Add(f);
                    }
                    if (f.leader == null) f.TryGenerateNewLeader();
                    Log.Message("[CalidoProbe] " + name + " leader " + f.leader?.def.defName);
                    foreach (var group in new[]{PawnGroupKindDefOf.Trader, PawnGroupKindDefOf.Combat})
                    {
                        var parms = new PawnGroupMakerParms { faction = f, tile = Find.AnyPlayerHomeMap.Tile, groupKind = group, points = 1800f };
                        var pawns = PawnGroupMakerUtility.GeneratePawns(parms).ToList();
                        Log.Message("[CalidoProbe] " + name + " " + group.defName + " " + string.Join(", ", pawns.Select(p => p.def.defName)));
                    }
                }
                catch (Exception e) { Log.Error("[CalidoProbe] " + name + " " + e); }
            }
            Log.Message("[CalidoProbe] Finished");
            UnityEngine.Application.Quit();
        }
    }
}
