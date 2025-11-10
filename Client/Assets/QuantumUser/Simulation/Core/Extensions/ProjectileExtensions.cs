using Photon.Deterministic;

namespace Quantum
{
    public static unsafe class ProjectileExtensions
    {
        public static void SpawnProjectile(this Frame frame, AssetRef<ProjectileData> projectileDataRef, FPVector2 position, FPVector2 direction, EntityRef owner)
        {
            frame.Signals.SpawnProjectile(projectileDataRef, position, direction, owner);
        }

        public static void SpawnSkillField(this Frame frame, AssetRef<SkillFieldData> skillFieldDataRef, FPVector2 position, EntityRef owner)
        {
            frame.Signals.SpawnSkillField(skillFieldDataRef, position, owner);
        }

        public static void DestroyProjectile(this Frame frame, EntityRef projectile, ProjectileDestroyReason reason)
        {
            frame.Signals.DestroyProjectile(projectile, reason);
        }

        public static void DestroySkillField(this Frame frame, EntityRef skillField)
        {
            frame.Signals.DestroySkillField(skillField);
        }
    }
}