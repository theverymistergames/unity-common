using System;
using MisterGames.Common.Data;

namespace MisterGames.Tick.Jobs {

    [Serializable]
    internal sealed class JobSystemWait : IJobSystemReadOnly {

        private readonly DictionaryList<int, ReadOnlyJob> _jobs = new DictionaryList<int, ReadOnlyJob>();
        private IJobIdFactory _jobIdFactory;

        public void Initialize(IJobIdFactory jobIdFactory) {
            _jobIdFactory = jobIdFactory;
        }

        public void DeInitialize() {
            _jobs.Clear();
        }

        public ReadOnlyJob CreateJob(ReadOnlyJob waitFor) {
            if (waitFor.IsCompleted) return Jobs.Completed;

            int jobId = _jobIdFactory.CreateNewJobId();
            _jobs.Add(jobId, waitFor);

            return new ReadOnlyJob(jobId, this);
        }

        public void DisposeJob(int jobId) {
            int index = _jobs.Keys.IndexOf(jobId);
            if (index >= 0) _jobs.RemoveAt(index);
        }

        public bool IsJobCompleted(int jobId) {
            int index = _jobs.Keys.IndexOf(jobId);
            if (index < 0) return true;

            if (!_jobs.Values[index].IsCompleted) return false;

            _jobs.RemoveAt(index);
            return true;
        }
    }

}
