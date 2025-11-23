using Photon.Deterministic;
using UnityEngine.Scripting;

namespace Quantum
{
    [Preserve]
    public unsafe class SkillActivationSystem : SystemSignalsOnly, ISignalOnSkillActivationRequested
    {
        public void OnSkillActivationRequested(Frame frame, EntityRef entityRef, AssetRef<SkillData> skillDataRef)
        {
#if DEBUG || UNITY_EDITOR
            UnityEngine.Debug.Log(
                $"[SkillActivationSystem] OnSkillActivationRequested - Entity: {entityRef}, SkillData: {skillDataRef.Id.Value}");
#endif

            if (!skillDataRef.Id.IsValid)
            {
#if DEBUG || UNITY_EDITOR
                UnityEngine.Debug.LogWarning($"[SkillActivationSystem] SkillData ID is invalid!");
#endif
                return;
            }

            SkillData skillData = frame.FindAsset<SkillData>(skillDataRef.Id);
            if (skillData == null)
            {
#if DEBUG || UNITY_EDITOR
                UnityEngine.Debug.LogWarning($"[SkillActivationSystem] SkillData is null!");
#endif
                return;
            }

#if DEBUG || UNITY_EDITOR
            UnityEngine.Debug.Log($"[SkillActivationSystem] SkillData loaded: {skillData.SkillName}");
#endif

            if (!skillData.CanActivate(frame, entityRef))
            {
#if DEBUG || UNITY_EDITOR
                UnityEngine.Debug.LogWarning($"[SkillActivationSystem] CanActivate returned false!");
#endif
                return;
            }

#if DEBUG || UNITY_EDITOR
            UnityEngine.Debug.Log($"[SkillActivationSystem] CanActivate passed!");
#endif

            if (!frame.Has<SkillComponent>(entityRef))
            {
                frame.Add<SkillComponent>(entityRef);
#if DEBUG || UNITY_EDITOR
                UnityEngine.Debug.Log($"[SkillActivationSystem] Added SkillComponent");
#endif
            }

            if (!frame.Unsafe.TryGetPointer<SkillComponent>(entityRef, out var skillComponent))
            {
#if DEBUG || UNITY_EDITOR
                UnityEngine.Debug.LogWarning($"[SkillActivationSystem] Failed to get SkillComponent pointer!");
#endif
                return;
            }

            if (skillComponent->CurrentSkill.Id.IsValid)
            {
                SkillData currentSkill = frame.FindAsset<SkillData>(skillComponent->CurrentSkill.Id);
                if (currentSkill != null && !currentSkill.Flags.HasFlag(SkillFlags.Cancelable))
                {
#if DEBUG || UNITY_EDITOR
                    UnityEngine.Debug.LogWarning(
                        $"[SkillActivationSystem] Current skill {currentSkill.SkillName} is not cancelable!");
#endif
                    return;
                }
            }

            skillComponent->CurrentSkill = skillDataRef;
            skillComponent->Phase = SkillPhase.Startup;
            skillComponent->PhaseTimer = FP._0;
            skillComponent->ElapsedTime = FP._0;
            skillComponent->ActionIndex = 0;
            skillComponent->HasTriggeredLanding = false;

            if (skillComponent->HitEntities.Ptr != default)
            {
                frame.FreeList(skillComponent->HitEntities);
            }

            skillComponent->HitEntities = frame.AllocateList<EntityRef>();

            skillData.OnSkillStarted(frame, entityRef);

            frame.Signals.OnSkillPhaseChanged(entityRef, SkillPhase.Startup);
            frame.Events.SkillActivated(entityRef, skillDataRef);

#if DEBUG || UNITY_EDITOR
            UnityEngine.Debug.Log($"[SkillActivationSystem] ✓ Skill {skillData.SkillName} activated successfully!");
#endif
        }
    }
}