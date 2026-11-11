using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace CalidoHomecoming
{
    internal static class PolearmEffects
    {
        internal static bool IsPolearm(ThingDef def)
        {
            return def?.defName == "Calido_ReachSpear" || def?.defName == "Calido_ReturningLantern";
        }

        internal static void Show(Map map, IntVec3 position, IntVec3 origin, bool thrust)
        {
            if (map == null || !position.IsValid) return;
            FleckDef arc = DefDatabase<FleckDef>.GetNamedSilentFail("CatHeroFX_QiSlash")
                ?? DefDatabase<FleckDef>.GetNamedSilentFail("CatHeroFX_Slash");
            FleckDef flash = DefDatabase<FleckDef>.GetNamedSilentFail(thrust ? "Ancot_FlashStab" : "CatHeroFX_Slash");
            Vector3 point = position.ToVector3Shifted();
            Vector3 direction = point - origin.ToVector3Shifted();
            float angle = direction.sqrMagnitude > 0.01f ? direction.AngleFlat() : 0f;
            if (arc != null)
            {
                FleckCreationData data = FleckMaker.GetDataStatic(point, map, arc, thrust ? 0.85f : 1.15f);
                data.rotation = angle + (thrust ? 90f : 35f);
                data.instanceColor = thrust ? new Color(0.8f, 0.9f, 1f) : new Color(1f, 0.85f, 0.5f);
                data.solidTimeOverride = 0.25f;
                map.flecks.CreateFleck(data);
            }
            if (flash != null)
            {
                FleckCreationData data = FleckMaker.GetDataStatic(point, map, flash, thrust ? 2f : 2.2f);
                data.rotation = angle;
                map.flecks.CreateFleck(data);
            }
        }
    }

    [HarmonyPatch(typeof(Bullet), "Impact")]
    internal static class PolearmThrustEffectPatch
    {
        private static void Prefix(Bullet __instance)
        {
            if (__instance.def.defName == "Calido_Projectile_ReachThrust")
                PolearmEffects.Show(__instance.Map, __instance.Position, __instance.Launcher?.Position ?? __instance.Position, true);
        }
    }

    [HarmonyPatch(typeof(Verb_MeleeAttack), "TryCastShot")]
    internal static class PolearmMeleeEffectPatch
    {
        private static void Postfix(Verb_MeleeAttack __instance, bool __result)
        {
            if (!__result || !PolearmEffects.IsPolearm(__instance.EquipmentSource?.def)) return;
            PolearmWeaponAnimation.Start(__instance.EquipmentSource, __instance.Caster.Position,
                __instance.CurrentTarget.Cell, __instance.EquipmentSource.def.defName == "Calido_ReachSpear");
            PolearmEffects.Show(__instance.Caster?.Map, __instance.CurrentTarget.Cell, __instance.Caster.Position,
                __instance.EquipmentSource.def.defName == "Calido_ReachSpear");
        }
    }
}
