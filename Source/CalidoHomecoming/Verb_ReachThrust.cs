using RimWorld;
using Verse;

namespace CalidoHomecoming
{
    // Keep the ranged verb for hunting and target selection, but resolve the
    // thrust immediately at the selected target instead of launching a bullet.
    public sealed class Verb_ReachThrust : Verb_Shoot
    {
        protected override bool TryCastShot()
        {
            Thing target = CurrentTarget.Thing;
            if (target == null || target.Destroyed || !target.Spawned || target.Map != Caster.Map)
                return false;

            PolearmEffects.Show(target.Map, target.Position, Caster.Position, true);
            float accuracy = EquipmentSource?.GetStatValue(StatDefOf.AccuracyShort) ?? 0.88f;
            if (Rand.Value <= accuracy)
            {
                var damage = new DamageInfo(DamageDefOf.Stab, 9f, 0.16f,
                    instigator: Caster, weapon: EquipmentSource?.def);
                target.TakeDamage(damage);
            }
            return true;
        }
    }
}
