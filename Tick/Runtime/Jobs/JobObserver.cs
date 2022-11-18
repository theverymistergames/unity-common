using System.Collections.Generic;
using MisterGames.Tick.Core;

namespace MisterGames.Tick.Jobs {
    
    public sealed class JobObserver : IJobReadOnly {

        public bool IsCompleted {
            get {
                RemoveCompletedJobs();
                return _jobs.Count == 0;
            }
        }

        private readonly List<IJobReadOnly> _jobs = new List<IJobReadOnly>();

        public void Observe(params IJobReadOnly[] jobs) {
            _jobs.AddRange(jobs);
            RemoveCompletedJobs();
        }

        public void StopAll() {
            for (int i = 0; i < _jobs.Count; i++) {
                if (_jobs[i] is IJob job) job.Stop();
            }
        }

        public void Clear() {
            _jobs.Clear();
        }

        private void RemoveCompletedJobs() {
            for (int i = _jobs.Count - 1; i >= 0; i--) {
                if (_jobs[i].IsCompleted) _jobs.RemoveAt(i);
            }
        }
    }
    
}
