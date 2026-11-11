using HarmonyLib;
using RimWorld;
using System.Reflection;
using Verse;

namespace CalidoHomecoming
{
    [HarmonyPatch]
    public static class HighTechCanEquipPatch
    {
        public static MethodBase TargetMethod() => AccessTools.Method(typeof(EquipmentUtility), "CanEquip", new[] { typeof(Thing), typeof(Pawn), typeof(string).MakeByRefType(), typeof(bool) });

        public static bool Prefix(Thing thing, Pawn pawn, ref string cantReason, ref bool __result)
        {
            CalidoStoryState state = Current.Game?.GetComponent<CalidoStoryState>();
            if (state?.ScenarioEnabled == true && !state.WeaponBlueprintUnlocked && CalidoStoryState.IsMilira(pawn) && CalidoStoryState.IsLockedMiliraTech(thing?.def))
            {
                cantReason = "無法裝備：尚未取得卡利多武器授權藍圖與設備校準。";
                __result = false;
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(ApparelUtility), nameof(ApparelUtility.HasPartsToWear))]
    public static class HighTechCanWearPatch
    {
        public static bool Prefix(Pawn p, ThingDef apparel, ref bool __result)
        {
            CalidoStoryState state = Current.Game?.GetComponent<CalidoStoryState>();
            if (state?.ScenarioEnabled == true && !state.ArmorBlueprintUnlocked && CalidoStoryState.IsMilira(p) && CalidoStoryState.IsLockedMiliraTech(apparel))
            {
                __result = false;
                return false;
            }
            return true;
        }
    }

    // HasPartsToWear has no reason output. Relabel its disabled right-click
    // option so the authorization restriction is not reported as an amputation.
    [HarmonyPatch]
    public static class HighTechWearReasonPatch
    {
        public static MethodBase TargetMethod() => AccessTools.Method(
            AccessTools.TypeByName("RimWorld.FloatMenuOptionProvider_Wear"),
            "GetSingleOptionFor", new[] { typeof(Thing), typeof(FloatMenuContext) });

        public static void Postfix(Thing clickedThing, FloatMenuContext context, ref FloatMenuOption __result)
        {
            Pawn pawn = context?.FirstSelectedPawn;
            CalidoStoryState state = Current.Game?.GetComponent<CalidoStoryState>();
            if (__result == null || !__result.Disabled || state?.ScenarioEnabled != true ||
                state.ArmorBlueprintUnlocked || !CalidoStoryState.IsMilira(pawn) ||
                !CalidoStoryState.IsLockedMiliraTech(clickedThing?.def)) return;

            __result.Label = "無法穿戴 " + clickedThing.LabelShort + "：尚未取得卡利多防具授權。";
        }
    }
}
