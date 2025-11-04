using Photon.Deterministic;

namespace Quantum
{
    public unsafe class DashAbilityData : AbilityData
    {
        public DashDirection DirectionType = DashDirection.Input;
        
        [RangeEx(5, 100)]
        public FP MaxDashSpeed;
        public FPAnimationCurve DashMovementCurve;
        // Dash结束时速度保留百分比 (0 = 完全停止, 1 = 保持全速)
        [RangeEx(0, 1)]
        public FP EndSpeedRetention = FP._0_25; // 保留25%的速度
        public bool DashSuspendsGravity;
        // 是否在结束时平滑过渡
        public bool SmoothEndTransition = true;
        
        public override Ability.AbilityState UpdateAbility(Frame frame, EntityRef entityRef, Ability* ability)
        {
            Ability.AbilityState abilityState = base.UpdateAbility(frame, entityRef, ability);

            // 处理技能激活时刻
            if (abilityState.IsActiveStartTick)
            {
                OnDashStart(frame, entityRef);
            }

            // 处理技能持续期间
            if (abilityState.IsActive)
            {
                UpdateDashMovement(frame, entityRef, ability);
            }
            
            // 处理技能结束时刻
            if (abilityState.IsActiveEndTick)
            {
                OnDashEnd(frame, entityRef);
            }

            return abilityState;
        }
        
        public override unsafe bool TryActivateAbility(Frame frame, EntityRef entityRef, PlayerLink* playerLink, AbilityType abilityType, ref Ability ability)
        {
            bool activated = base.TryActivateAbility(frame, entityRef, playerLink, abilityType, ref ability);

            if (activated)
            {
                // dosometing
            }

            return activated;
        }
        
        // Dash开始时的初始化
        private void OnDashStart(Frame frame, EntityRef entityRef)
        {
            var abilityEnable = frame.Unsafe.GetPointer<AbilityEnable>(entityRef);

            bool dashEnabled = abilityEnable->MovementDashEnabled;
            if (!dashEnabled)
            {
                return;
            }

            var kcc = frame.Unsafe.GetPointer<KCC2D>(entityRef);
            var movementData = frame.Unsafe.GetPointer<MovementData>(entityRef);
            // 设置Dash状态
            kcc->SetState(frame, KCCState.DASHING, Duration);

            // 计算Dash方向和初始速度
            FP dashDirection = GetDashDirection(kcc, movementData);

            // 使用曲线在开始时刻(t=0)的速度
            FP speedMultiplier = DashMovementCurve.Samples.Length > 0 
                ? DashMovementCurve.Evaluate(FP._0) 
                : FP._1;
                
            kcc->KinematicHorizontalSpeed = dashDirection * MaxDashSpeed * speedMultiplier;

            // 挂起重力（如果配置了）
            if (DashSuspendsGravity)
            {
                kcc->KinematicVerticalSpeed = 0;
            }
        }

        // Dash期间每帧更新速度（基于曲线）
        private void UpdateDashMovement(Frame frame, EntityRef entityRef, Ability* ability)
        {
            var kcc = frame.Unsafe.GetPointer<KCC2D>(entityRef);
            
            if (kcc->State != KCCState.DASHING)
            {
                return;
            }

            // 检查是否有有效的曲线
            if (DashMovementCurve.Samples == null || DashMovementCurve.Samples.Length == 0)
            {
                return;
            }
            
            var movementData = frame.Unsafe.GetPointer<MovementData>(entityRef);

            // 获取当前Dash的归一化时间 (0-1)
            FP normalizedTime = ability->DurationTimer.NormalizedTime;
            
            // 从曲线获取当前速度倍数
            FP speedMultiplier = DashMovementCurve.Evaluate(normalizedTime);
            
            // 保持Dash方向，但根据曲线调整速度大小
            FP dashDirection = GetDashDirection(kcc, movementData);
            kcc->KinematicHorizontalSpeed = dashDirection * MaxDashSpeed * speedMultiplier;
            
            // 如果启用了重力挂起，持续保持垂直速度为0
            if (DashSuspendsGravity)
            {
                kcc->KinematicVerticalSpeed = 0;
            }
        }
        
        // Dash结束时的速度处理
        private void OnDashEnd(Frame frame, EntityRef entityRef)
        {
            var kcc = frame.Unsafe.GetPointer<KCC2D>(entityRef);
            var abilityInventory = frame.Unsafe.GetPointer<AbilityInventory>(entityRef);

            if (SmoothEndTransition)
            {
                // 方案1: 保留部分冲刺速度，实现平滑过渡
                // 当前速度乘以保留百分比
                kcc->KinematicHorizontalSpeed *= EndSpeedRetention;
            }
            else
            {
                // 方案2: 恢复到施放技能前的速度
                // 使用保存的CastVelocity
                FPVector2 originalVelocity = abilityInventory->ActiveAbilityInfo.CastVelocity;
                kcc->_kinematicVelocity = originalVelocity * EndSpeedRetention;
            }
            
            // 退出Dashing状态，让KCC系统自动切换到合适的状态
            // 不需要手动设置状态，ComputeState会处理
        }
        
        private FP GetDashDirection(KCC2D* kcc, MovementData* movementData)
        {
            FP direction = FP._0;
    
            switch (DirectionType)
            {
                case DashDirection.Velocity:
                    // 基于当前速度方向
                    if (kcc->CombinedVelocity.X != FP._0)
                    {
                        direction = FPMath.Sign(kcc->CombinedVelocity.X);
                    }
                    break;
            
                case DashDirection.Input:
                    // 基于最后的输入方向
                    if (kcc->LastInputDirection != 0)
                    {
                        direction = kcc->LastInputDirection; // int自动转FP
                    }
                    break;
            }
    
            // ✓ 如果没有有效方向，使用角色朝向
            if (direction == FP._0)
            {
                direction = movementData->IsFacingRight ? FP._1 : -FP._1;
            }
    
            return direction;
        }

    }
}