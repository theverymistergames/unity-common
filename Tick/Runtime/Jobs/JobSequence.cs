using System.Collections.Generic;
using MisterGames.Tick.Core;

namespace MisterGames.Tick.Jobs {
    
    public sealed class JobSequence : IJob, IUpdate {

        public bool IsCompleted => _jobs.Count == 0;

        private readonly Queue<IJob> _jobs = new Queue<IJob>();

        private bool _isUpdating;

        private JobSequence() { }

        public static JobSequence Create() {
            return new JobSequence();
        }

        public JobSequence Add(IJob job) {
            if (!job.IsCompleted) _jobs.Enqueue(job);
            return this;
        }

        public void Start() {
            if (IsCompleted) return;

            StartJobsTillUnableToComplete();
            _isUpdating = _jobs.Count > 0;
        }

        public void Stop() {
            _isUpdating = false;
            if (_jobs.TryPeek(out var job)) job.Stop();
        }

        void IUpdate.OnUpdate(float dt) {
            if (!_isUpdating) return;

            if (!_jobs.TryPeek(out var job)) {
                _isUpdating = false;
                return;
            }

            if (job.IsCompleted) {
                StartJobsTillUnableToComplete();
                _isUpdating = _jobs.Count > 0;
                return;
            }

            if (job is IUpdate update) update.OnUpdate(dt);

            if (job.IsCompleted) {
                StartJobsTillUnableToComplete();
                _isUpdating = _jobs.Count > 0;
            }
        }

        private void StartJobsTillUnableToComplete() {
            while (_jobs.TryPeek(out var job)) {
                job.Start();
                if (!job.IsCompleted) return;

                _jobs.Dequeue();
            }
        }
    }
    
}
