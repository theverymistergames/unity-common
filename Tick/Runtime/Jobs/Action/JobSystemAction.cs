using System;
using MisterGames.Common.Data;

namespace MisterGames.Tick.Jobs {

    [Serializable]
    internal sealed class JobSystemAction : IJobSystem {

        private readonly DictionaryList<int, Action> _jobs = new DictionaryList<int, Action>();
        private IJobIdFactory _jobIdFactory;

        public void Initialize(IJobIdFactory jobIdFactory) {
            _jobIdFactory = jobIdFactory;
        }

        public void DeInitialize() {
            _jobs.Clear();
        }

        public Job CreateJob(Action action) {
            int jobId = _jobIdFactory.CreateNewJobId();
            _jobs.Add(jobId, action);

            return new Job(jobId, this);
        }

        public bool IsJobCompleted(int jobId) {
            return _jobs.Keys.IndexOf(jobId) < 0;
        }

        public void StartJob(int jobId) {
            int index = _jobs.Keys.IndexOf(jobId);
            if (index < 0) return;

            _jobs.Values[index].Invoke();
            _jobs.RemoveAt(index);
        }

        public void StopJob(int jobId) { }
    }

}
