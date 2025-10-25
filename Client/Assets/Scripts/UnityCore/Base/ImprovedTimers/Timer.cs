using System;
using UnityEngine;

namespace UnityCore.Base.ImprovedTimers
{
    public class TestTimer : Timer
    {
        public TestTimer(float value) : base(value)
        {
        }

        public override void Tick()
        {
            if (!IsRunning) return;
            
            CurrentTime -= Time.deltaTime;
            if (IsFinished)
            {
                Stop();
            }
        }

        public override bool IsFinished => CurrentTime <= 0f;
    }
    
    public abstract class Timer : IDisposable
    {
        public float CurrentTime { get; protected set; }
        public bool IsRunning { get; private set; }

        protected float InitialTime;
        
        public float Progress => Mathf.Clamp(CurrentTime / InitialTime, 0f, 1f);

        public Action OnTimerStart;
        public Action OnTimerEnd;

        protected Timer(float value)
        {
            InitialTime = value;
        }

        public void Start()
        {
            CurrentTime = InitialTime;
            if (!IsRunning)
            {
                IsRunning = true;
                TimerManager.RegisterTimer(this);
                OnTimerStart?.Invoke();
            }
        }

        public void Stop()
        {
            if (IsRunning)
            {
                IsRunning = false;
                TimerManager.DeregisterTimer(this);
                OnTimerEnd?.Invoke();
            } 
        }
        
        public abstract void Tick();
        public abstract bool IsFinished { get; }
        
        public void Resume() => IsRunning = true;
        public void Pause() => IsRunning = false;
        
        public virtual void Reset() => CurrentTime = InitialTime;

        public virtual void Reset(float newTime)
        {
            InitialTime = newTime;
            Reset();
        }

        bool _disposed = false;
        
        ~Timer()
        {
            Dispose(false);
        }
        
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }
            
            if (disposing)
            {
                TimerManager.DeregisterTimer(this);
            }
            
            _disposed = true;
        }
    }
}