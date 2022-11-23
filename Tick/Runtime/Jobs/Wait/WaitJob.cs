using System.Collections.Generic;
using MisterGames.Tick.Core;

namespace MisterGames.Tick.Jobs {
    
    internal sealed class WaitAllJob : IJob, IUpdate {

        public bool IsCompleted => _jobs.Count == 0;

        private readonly List<IJobReadOnly> _jobs = new List<IJobReadOnly>();
        private bool _isUpdating;

        public WaitAllJob(params IJobReadOnly[] jobs) {
            for (int i = 0; i < jobs.Length; i++) {
                var job = jobs[i];
                if (!job.IsCompleted) _jobs.Add(job);
            }
        }

        public void Start() {
            for (int i = _jobs.Count - 1; i >= 0; i--) {
                if (_jobs[i].IsCompleted) _jobs.RemoveAt(i);
            }

            _isUpdating = _jobs.Count > 0;
        }

        public void Stop() {
            _isUpdating = false;
        }

        public void OnUpdate(float dt) {
            if (!_isUpdating) return;

            for (int i = _jobs.Count - 1; i >= 0; i--) {
                if (_jobs[i].IsCompleted) _jobs.RemoveAt(i);
            }

            if (IsCompleted) _isUpdating = false;
        }
    }

    internal sealed class WaitJob<R> : IJob<R>, IUpdate {

        public bool IsCompleted => _resultJob.IsCompleted;
        public R Result => _resultJob.Result;

        private readonly IJobReadOnly<R> _resultJob;
        private bool _isUpdating;

        public WaitJob(IJobReadOnly<R> job) {
            _resultJob = job;
        }

        public void Start() {
            _isUpdating = !_resultJob.IsCompleted;
        }

        public void Stop() {
            _isUpdating = false;
        }

        public void OnUpdate(float dt) {
            if (!_isUpdating) return;

            if (_resultJob.IsCompleted) _isUpdating = false;
        }
    }
}
