using RimWorld;
using Verse;

namespace CalidoHomecoming
{
    public sealed class StatPart_WaystationHauling : StatPart
    {
        private static bool IsHauling(StatRequest request)
        {
            if (!(request.Thing is Pawn pawn) || pawn.carryTracker?.CarriedThing == null) return false;
            // Carrying another person or animal is rescue, not hauling supplies.
            if (pawn.carryTracker.CarriedThing is Pawn) return false;
            HediffDef specialty = DefDatabase<HediffDef>.GetNamedSilentFail("Calido_Hero_Waystation");
            return specialty != null && pawn.health?.hediffSet?.HasHediff(specialty) == true;
        }

        public override void TransformValue(StatRequest req, ref float val)
        {
            if (IsHauling(req)) val *= 5f;
        }

        public override string ExplanationPart(StatRequest req)
        {
            return IsHauling(req) ? "遠棧：搬運物資移動速度 ×500%" : null;
        }
    }
}
