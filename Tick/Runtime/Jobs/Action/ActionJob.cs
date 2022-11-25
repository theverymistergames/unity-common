using System;

namespace MisterGames.Tick.Jobs {
    
    internal sealed class ActionJob : IJob {

        public bool IsCompleted { get; private set; }
        public float Progress => IsCompleted ? 1f : 0f;

        private readonly Action _action;

        public ActionJob(Action action) {
            _action = action;
        }

        public void Start() {
            if (IsCompleted) return;

            _action.Invoke();
            IsCompleted = true;
        }

        public void Stop() { }
    }

    internal sealed class ActionJob<R> : IJob<R> {

        public bool IsCompleted { get; private set; }
        public float Progress => IsCompleted ? 1f : 0f;
        public R Result { get; private set; }

        private readonly Func<R> _func;

        public ActionJob(Func<R> func) {
            _func = func;
        }

        public void Start() {
            if (IsCompleted) return;

            Result = _func.Invoke();
            IsCompleted = true;
        }

        public void Stop() { }
    }

}
