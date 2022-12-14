using System;
using MisterGames.Common.Data;
using MisterGames.Tick.Core;

namespace MisterGames.Tick.Jobs {

    [Serializable]
    public sealed class JobSystemEachFrameWhile : IJobSystem, IUpdate {

        private readonly DictionaryList<int, JobData> _jobs = new DictionaryList<int, JobData>();
        private IJobIdFactory _jobIdFactory;

        public void Initialize(IJobIdFactory jobIdFactory) {
            _jobIdFactory = jobIdFactory;
        }

        public void DeInitialize() {
            _jobs.Clear();
        }

        public Job CreateJob(Func<float, bool> action) {
            int jobId = _jobIdFactory.CreateNewJobId();
            _jobs.Add(jobId, new JobData(action));

            return new Job(jobId, this);
        }

        public bool IsJobCompleted(int jobId) {
            int index = _jobs.Keys.IndexOf(jobId);
            return index < 0;
        }

        public void StartJob(int jobId) {
            int index = _jobs.Keys.IndexOf(jobId);
            if (index >= 0) _jobs.Values[index] = _jobs.Values[index].Start();
        }

        public void StopJob(int jobId) {
            int index = _jobs.Keys.IndexOf(jobId);
            if (index >= 0) _jobs.Values[index] = _jobs.Values[index].Stop();
        }

        public void DisposeJob(int jobId) {
            int index = _jobs.Keys.IndexOf(jobId);
            if (index >= 0) _jobs.Values[index] = JobData.Completed;
        }

        public void OnUpdate(float dt) {
            int count = _jobs.Count;
            for (int i = 0; i < count; i++) {
                var job = _jobs.Values[i];

                if (job.isCompleted) {
                    _jobs.RemoveAt(i--);
                    count--;
                    continue;
                }

                if (!job.isUpdating || job.actionWhile.Invoke(dt)) continue;

                _jobs.RemoveAt(i--);
                count--;
            }
        }

        private readonly struct JobData {

            public static readonly JobData Completed = new JobData(null, false, true);

            public readonly bool isUpdating;
            public readonly bool isCompleted;
            public readonly Func<float, bool> actionWhile;

            public JobData(Func<float, bool> actionWhile, bool isUpdating = false, bool isCompleted = false) {
                this.actionWhile = actionWhile;
                this.isUpdating = isUpdating;
                this.isCompleted = isCompleted;
            }

            public JobData Start() {
                return new JobData(actionWhile, !isCompleted, isCompleted);
            }

            public JobData Stop() {
                return new JobData(actionWhile, false, isCompleted);
            }
        }
    }

}
