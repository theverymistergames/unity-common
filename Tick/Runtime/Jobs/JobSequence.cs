using System.Collections.Generic;
using MisterGames.Tick.Core;

namespace MisterGames.Tick.Jobs {
    
    public sealed class JobSequence : IJob, IUpdate {

        public bool IsCompleted => _isCompleted;

        private readonly Queue<IJob> _jobs = new Queue<IJob>();

        private bool _isUpdating;
        private bool _isCompleted;

        private JobSequence() { }

        public static JobSequence Create() {
            return new JobSequence();
        }

        public JobSequence Add(IJob job) {
            if (job.IsCompleted) _jobs.Enqueue(job);
            return this;
        }

        public void Start() {
            if (_isCompleted) return;

            StartJobsTillUnableToComplete();

            if (_jobs.Count == 0) {
                SetCompleted();
                return;
            }

            SetUpdating();
        }

        public void Stop() {
            _isUpdating = false;
            if (_jobs.TryPeek(out var job)) job.Stop();
        }

        void IUpdate.OnUpdate(float dt) {
            if (!_isUpdating) return;

            if (!_jobs.TryPeek(out var job)) {
                SetCompleted();
                return;
            }

            if (job is IUpdate update) update.OnUpdate(dt);
            if (!job.IsCompleted) return;

            StartJobsTillUnableToComplete();
            if (_jobs.Count == 0) SetCompleted();
        }

        private void StartJobsTillUnableToComplete() {
            while (_jobs.TryPeek(out var job)) {
                job.Start();
                if (!job.IsCompleted) return;

                _jobs.Dequeue();
            }
        }

        private void SetUpdating() {
            _isUpdating = true;
            _isCompleted = false;
        }

        private void SetCompleted() {
            _isUpdating = false;
            _isCompleted = true;
        }
    }
    
}
