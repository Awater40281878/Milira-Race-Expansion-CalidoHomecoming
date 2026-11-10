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
}
