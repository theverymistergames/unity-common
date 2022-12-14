using System;
using MisterGames.Common.Data;
using MisterGames.Tick.Core;

namespace MisterGames.Tick.Jobs {

    [Serializable]
    public sealed class JobSystemObserver : IJobSystem {

        private readonly DictionaryList<int, ReadOnlyJob> _jobs = new DictionaryList<int, ReadOnlyJob>();
        private IJobIdFactory _jobIdFactory;

        public void Initialize(IJobIdFactory jobIdFactory) {
            _jobIdFactory = jobIdFactory;
        }

        public void DeInitialize() {
            _jobs.Clear();
        }

        public int CreateJob() {
            return _jobIdFactory.CreateNewJobId();
        }

        public void Observe(int observerJobId, ReadOnlyJob job) {
            if (job.IsCompleted) return;

            _jobs.Add(observerJobId, job);
        }

        public bool IsJobCompleted(int jobId) {
            int firstIndex = _jobs.Keys.IndexOf(jobId);
            if (firstIndex < 0) return true;

            int lastIndex = _jobs.Keys.LastIndexOf(jobId);
            for (int i = lastIndex; i >= firstIndex; i--) {
                if (jobId != _jobs.Keys[i]) continue;

                if (!_jobs.Values[i].IsCompleted) return false;

                _jobs.RemoveAt(i);
            }

            return true;
        }

        public void StartJob(int jobId) { }

        public void StopJob(int jobId) { }

        public void DisposeJob(int jobId) {
            int firstIndex = _jobs.Keys.IndexOf(jobId);
            if (firstIndex < 0) return;

            int lastIndex = _jobs.Keys.LastIndexOf(jobId);
            for (int i = lastIndex; i >= firstIndex; i--) {
                if (jobId != _jobs.Keys[i]) continue;

                _jobs.RemoveAt(i);
            }
        }
    }

}
