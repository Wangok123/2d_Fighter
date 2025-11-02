using Photon.Deterministic;
using Quantum.Physics2D;

namespace Quantum
{
  // partial extensions to the component struct
  unsafe partial struct KCC2D
  {

    public FPVector2 CombinedVelocity
    {
      get { return _kinematicVelocity + _dynamicVelocity; }
    }

    public KCCState State
    {
      get { return _state; }
    }

    public FrameTimer StateTimer
    {
      get { return _stateTimer; }
    }

    public void SetState(Frame f, KCCState state, FP? time = null)
    {
      _state = state;
      if (time.HasValue)
        _stateTimer = FrameTimer.FromSeconds(f, time.Value);
    }

    public void SetStateTimer(Frame f, FP time)
    {
        _stateTimer = FrameTimer.FromSeconds(f, time);
    }

    public FPVector2 DynamicVelocity
    {
      set { _dynamicVelocity = value; }
    }

    public FP KinematicHorizontalSpeed
    {
      get { return _kinematicVelocity.X; }
      set { _kinematicVelocity.X = value; }
    }

    public FP KinematicVerticalSpeed
    {
      get { return _kinematicVelocity.Y; }
      set { _kinematicVelocity.Y = value; }
    }

    public void ApplyKinematicAcceleration(Frame f, FPVector2 acceleration)
    {
      _kinematicVelocity += f.DeltaTime * acceleration;
    }

    public void ResetAll(Frame f, EntityRef entity)
    {
      _dynamicVelocity = default;
      _kinematicVelocity = default;
      SetState(f, KCCState.FREE_FALLING);
      _stateTimer = default;
      IgnoreStep = true;
      GroundedJumpTimer = default;
      Input = default;
      LastInputDirection = default;
      if (f.Unsafe.TryGetPointer<Transform2D>(entity, out var transform))
      {
        transform->Teleport(f, InitialPosition);
      }
    }


    public void Jump(Frame f, EntityRef entity, FPVector2 impulse)
    {
      _kinematicVelocity = impulse;
    }

    /// <summary>
    /// Gets the current grounded state of the character.
    /// Useful for ability systems to check if character is on ground.
    /// </summary>
    public bool IsGrounded
    {
      get { return _state == KCCState.GROUNDED; }
    }

    /// <summary>
    /// Gets the current airborne state of the character.
    /// Useful for ability systems to check if character is in air.
    /// </summary>
    public bool IsAirborne
    {
      get 
      { 
        return _state == KCCState.JUMPED || 
               _state == KCCState.DOUBLE_JUMPED || 
               _state == KCCState.FREE_FALLING; 
      }
    }

    /// <summary>
    /// Checks if the character is currently dashing.
    /// </summary>
    public bool IsDashing
    {
      get { return _state == KCCState.DASHING; }
    }

    /// <summary>
    /// Checks if the character is on a wall.
    /// </summary>
    public bool IsWalled
    {
      get { return _state == KCCState.WALLED; }
    }

    /// <summary>
    /// Applies an impulse force to the character.
    /// Useful for knockback, wind, or other external forces.
    /// </summary>
    public void ApplyImpulse(FPVector2 impulse)
    {
      _kinematicVelocity += impulse;
    }

    /// <summary>
    /// Applies a dynamic velocity (non-kinematic) to the character.
    /// This velocity is separate from kinematic movement and can be used for physics effects.
    /// </summary>
    public void ApplyDynamicVelocity(FPVector2 velocity)
    {
      _dynamicVelocity += velocity;
    }
  }
}

