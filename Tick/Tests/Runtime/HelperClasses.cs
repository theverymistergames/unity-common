using System;
using MisterGames.Tick.Core;
using MisterGames.Tick.Jobs;
using MisterGames.Tick.TimeProviders;

namespace Core {

    public readonly struct ConstantTimeProvider : ITimeProvider {
        public float UnscaledDeltaTime { get; }
        public ConstantTimeProvider(float value) => UnscaledDeltaTime = value;
    }

    public class CountOnUpdate : IUpdate {
        public int Count { get; private set; }
        public void OnUpdate(float dt) => Count++;
    }

    public class ActionOnUpdate : IUpdate {
        private readonly Action<IUpdate> _action;
        public ActionOnUpdate(Action<IUpdate> action) => _action = action;
        public void OnUpdate(float dt) => _action?.Invoke(this);
    }

    public class ActionOnStartJob : IJob {
        public bool IsCompleted { get; private set; }
        public bool IsStarted { get; private set; }
        public bool IsStopped { get; private set; }
        private readonly Action<ActionOnStartJob> _action;
        public ActionOnStartJob(Action<ActionOnStartJob> action = null) => _action = action;
        public void Start() {
            _action?.Invoke(this);
            IsStarted = true;
        }
        public void Stop() => IsStopped = true;
        public void ForceComplete() => IsCompleted = true;
    }

    public class ActionOnUpdateJob : IJob, IUpdate {
        public bool IsCompleted { get; private set; }
        public bool IsStarted { get; private set; }
        public bool IsStopped { get; private set; }
        private readonly Action<ActionOnUpdateJob> _action;
        public ActionOnUpdateJob(Action<ActionOnUpdateJob> action = null) => _action = action;
        public void Start() => IsStarted = true;
        public void Stop() => IsStopped = true;
        public void OnUpdate(float dt) => _action?.Invoke(this);
        public void ForceComplete() => IsCompleted = true;
    }

    public class CountOnUpdateJob : IJob, IUpdate {
        public bool IsCompleted { get; private set; }
        public bool IsStarted { get; private set; }
        public bool IsStopped { get; private set; }
        public int Count { get; private set; }
        public void Start() => IsStarted = true;
        public void Stop() => IsStopped = true;
        public void OnUpdate(float dt) => Count++;
        public void ForceComplete() => IsCompleted = true;
    }
}
