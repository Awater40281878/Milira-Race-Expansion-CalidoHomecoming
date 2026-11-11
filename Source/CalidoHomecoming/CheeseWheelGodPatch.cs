using System.Linq;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace CalidoHomecoming
{
    [HarmonyPatch(typeof(Thing), nameof(Thing.Ingested))]
    internal static class CheeseWheelGodPatch
    {
        private static void Prefix(Pawn ingester, out float __state)
        {
            Need_Food food = ingester?.needs?.food;
            __state = food == null ? 0f : Mathf.Max(0f, food.MaxLevel - food.CurLevel);
        }

        private static void Postfix(Pawn ingester, float __result, float __state)
        {
            if (ingester == null || __result <= 0f || __state <= 0f) return;
            CalidoStoryState story = Current.Game?.GetComponent<CalidoStoryState>();
            HediffDef blessing = DefDatabase<HediffDef>.GetNamedSilentFail("Calido_CheeseWheelGod");
            if (story?.IsProtagonist(ingester) != true || blessing == null ||
                ingester.health?.hediffSet?.HasHediff(blessing) != true) return;

            float remaining = Mathf.Min(__result, __state) * 60f;
            float healed = 0f;
            var injuries = ingester.health.hediffSet.hediffs.OfType<Hediff_Injury>()
                .Where(injury => injury.Severity > 0f &&
                    injury.TryGetComp<HediffComp_GetsPermanent>()?.IsPermanent != true)
                .OrderByDescending(injury => injury.Bleeding)
                .ThenByDescending(injury => injury.Severity).ToList();

            foreach (Hediff_Injury injury in injuries)
            {
                if (remaining <= 0f) break;
                float amount = Mathf.Min(remaining, injury.Severity);
                injury.Heal(amount);
                healed += amount;
                remaining -= amount;
            }

            if (healed > 0f && ingester.Spawned)
                MoteMaker.ThrowText(ingester.DrawPos, ingester.Map,
                    "起司輪之力！回復 +" + healed.ToString("0.0"),
                    new Color(1f, 217f / 255f, 51f / 255f), 3.65f);
        }
    }
}
