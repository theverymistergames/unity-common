using System;
using MisterGames.Common.Data;
using UnityEngine;

namespace MisterGames.Tick.Jobs {

    [Serializable]
    internal sealed class JobSystemAsyncOperation : IJobSystemReadOnly {

        private readonly DictionaryList<int, AsyncOperation> _jobs = new DictionaryList<int, AsyncOperation>();
        private IJobIdFactory _jobIdFactory;

        public void Initialize(IJobIdFactory jobIdFactory) {
            _jobIdFactory = jobIdFactory;
        }

        public void DeInitialize() {
            _jobs.Clear();
        }

        public ReadOnlyJob CreateJob(AsyncOperation asyncOperation) {
            if (asyncOperation.isDone) return Jobs.Completed;

            int jobId = _jobIdFactory.CreateNewJobId();
            _jobs.Add(jobId, asyncOperation);

            return new ReadOnlyJob(jobId, this);
        }

        public void DisposeJob(int jobId) {
            int index = _jobs.Keys.IndexOf(jobId);
            if (index >= 0) _jobs.RemoveAt(index);
        }

        public bool IsJobCompleted(int jobId) {
            int index = _jobs.Keys.IndexOf(jobId);
            if (index < 0) return true;

            if (!_jobs.Values[index].isDone) return false;

            _jobs.RemoveAt(index);
            return true;
        }
    }

}
