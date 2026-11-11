using HarmonyLib;
using RimWorld;
using Verse;

namespace CalidoHomecoming
{
    [HarmonyPatch(typeof(Pawn), nameof(Pawn.Kill))]
    public static class ProtagonistDeathPatch
    {
        public static void Postfix(Pawn __instance)
        {
            if (__instance?.Dead != true || __instance.story?.traits?.HasTrait(DefDatabase<TraitDef>.GetNamedSilentFail("Calido_ProtagonistAura")) != true) return;
            Current.Game?.GetComponent<CalidoStoryState>()?.NotifyProtagonistDeath(__instance);
        }
    }
}
