using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using Verse;

namespace CalidoHomecoming
{
    internal static class PolearmWeaponAnimation
    {
        private sealed class Swing
        {
            public float Started;
            public float Angle;
            public bool Thrust;
        }

        private static readonly Dictionary<Thing, Swing> Active = new Dictionary<Thing, Swing>();
        private const float Duration = 0.32f;

        public static void Start(Thing weapon, IntVec3 origin, IntVec3 target, bool thrust)
        {
            if (weapon == null || !origin.IsValid || !target.IsValid) return;
            Vector3 direction = target.ToVector3Shifted() - origin.ToVector3Shifted();
            if (direction.sqrMagnitude < 0.01f) return;
            if (Active.Count > 128) Active.Clear();
            Active[weapon] = new Swing { Started = Time.realtimeSinceStartup, Angle = direction.AngleFlat(), Thrust = thrust };
        }

        public static void Adjust(Thing weapon, ref Vector3 drawLoc, ref float aimAngle)
        {
            if (weapon == null || !Active.TryGetValue(weapon, out Swing swing)) return;
            float progress = (Time.realtimeSinceStartup - swing.Started) / Duration;
            if (progress >= 1f || progress < 0f)
            {
                Active.Remove(weapon);
                return;
            }

            float motion = Mathf.Sin(progress * Mathf.PI);
            aimAngle = swing.Angle + (swing.Thrust ? 0f : Mathf.Lerp(-55f, 55f, progress));
            if (swing.Thrust)
            {
                Vector3 direction = Quaternion.AngleAxis(swing.Angle, Vector3.up) * Vector3.forward;
                drawLoc += direction * (0.45f * motion);
            }
        }
    }

    [HarmonyPatch(typeof(PawnRenderUtility), nameof(PawnRenderUtility.DrawEquipmentAiming))]
    internal static class PolearmWeaponDrawPatch
    {
        private static void Prefix(Thing eq, ref Vector3 drawLoc, ref float aimAngle)
        {
            if (PolearmEffects.IsPolearm(eq?.def))
                PolearmWeaponAnimation.Adjust(eq, ref drawLoc, ref aimAngle);
        }
    }
}
