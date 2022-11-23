using System.Collections.Generic;

namespace MisterGames.Tick.Jobs {
    
    public sealed class JobObserver : IJobReadOnly {

        public bool IsCompleted => CheckCompletion();

        private readonly List<IJobReadOnly> _jobs = new List<IJobReadOnly>();

        public void Observe(IJobReadOnly job) {
            _jobs.Add(job);
            RemoveCompletedJobs();
        }

        public void ObserveAll(params IJobReadOnly[] jobs) {
            _jobs.AddRange(jobs);
            RemoveCompletedJobs();
        }

        public void Clear() {
            _jobs.Clear();
        }

        private bool CheckCompletion() {
            RemoveCompletedJobs();
            return _jobs.Count == 0;
        }

        private void RemoveCompletedJobs() {
            for (int i = _jobs.Count - 1; i >= 0; i--) {
                if (_jobs[i].IsCompleted) _jobs.RemoveAt(i);
            }
        }
    }
    
}
