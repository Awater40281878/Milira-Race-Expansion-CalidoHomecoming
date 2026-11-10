using HarmonyLib;
using Verse;

namespace CalidoHomecoming
{
    [HarmonyPatch(typeof(Verb), "get_EffectiveRange")]
    public static class CalidoSniperRangePatch
    {
        public static void Postfix(Verb __instance, ref float __result)
        {
            Pawn pawn = __instance?.CasterPawn;
            HediffDef sniper = DefDatabase<HediffDef>.GetNamedSilentFail("Calido_Hero_Sniper");
            if (pawn != null && sniper != null && pawn.health?.hediffSet?.HasHediff(sniper) == true && __result > 1.9f)
                __result *= 2f;
        }
    }
}
