using System;
using MisterGames.Common.Data;
using MisterGames.Tick.Core;

namespace MisterGames.Tick.Jobs {

    [Serializable]
    internal sealed class JobSystemDelay : IJobSystem, IUpdate {

        private readonly DictionaryList<int, JobData> _jobs = new DictionaryList<int, JobData>();
        private IJobIdFactory _jobIdFactory;

        public void Initialize(IJobIdFactory jobIdFactory) {
            _jobIdFactory = jobIdFactory;
        }

        public void DeInitialize() {
            _jobs.Clear();
        }

        public Job CreateJob(float delay) {
            var jobData = new JobData(delay);
            if (jobData.IsCompleted) return Jobs.Completed;

            int jobId = _jobIdFactory.CreateNewJobId();
            _jobs.Add(jobId, jobData);

            return new Job(jobId, this);
        }

        public bool IsJobCompleted(int jobId) {
            int index = _jobs.Keys.IndexOf(jobId);
            return index < 0 || _jobs.Values[index].IsCompleted;
        }

        public void StartJob(int jobId) {
            int index = _jobs.Keys.IndexOf(jobId);
            if (index >= 0) _jobs.Values[index] = _jobs.Values[index].Start();
        }

        public void StopJob(int jobId) {
            int index = _jobs.Keys.IndexOf(jobId);
            if (index >= 0) _jobs.Values[index] = _jobs.Values[index].Stop();
        }

        public void OnUpdate(float dt) {
            for (int i = _jobs.Count - 1; i >= 0; i--) {
                var job = _jobs.Values[i];

                if (job.IsCompleted) {
                    _jobs.RemoveAt(i);
                    continue;
                }

                if (!job.isUpdating) continue;

                _jobs.Values[i] = job.AddToTimer(dt);
            }
        }

        private readonly struct JobData {

            public bool IsCompleted => _timer >= _delay;
            public readonly bool isUpdating;

            private readonly float _delay;
            private readonly float _timer;

            public JobData(float delay, float timer = 0f, bool isUpdating = false) {
                _delay = delay;
                _timer = timer;
                this.isUpdating = isUpdating;
            }

            public JobData AddToTimer(float value) {
                return new JobData(_delay, _timer + value, isUpdating);
            }

            public JobData Start() {
                return new JobData(_delay, _timer, _timer < _delay);
            }

            public JobData Stop() {
                return new JobData(_delay, _timer, false);
            }
        }
    }

}
