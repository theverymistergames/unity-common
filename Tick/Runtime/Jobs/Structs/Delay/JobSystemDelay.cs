using System;
using MisterGames.Common.Data;
using MisterGames.Tick.Core;

namespace MisterGames.Tick.Jobs.Structs {

    [Serializable]
    internal sealed class JobSystemDelay : IJobSystem<float>, IUpdate {

        private readonly DictionaryList<int, DelayJobData> _delayJobs = new DictionaryList<int, DelayJobData>();
        private IJobIdFactory _jobIdFactory;

        public void Initialize(IJobIdFactory jobIdFactory) {
            _jobIdFactory = jobIdFactory;
        }

        public void DeInitialize() {
            _delayJobs.Clear();
        }

        public int CreateJob(float data) {
            var jobData = DelayJobData.Create(data);
            if (jobData.IsCompleted) return -1;

            int jobId = _jobIdFactory.CreateNewJobId();
            _delayJobs.Add(jobId, jobData);

            return jobId;
        }

        public bool IsJobCompleted(int jobId) {
            int index = _delayJobs.IndexOf(jobId);
            return index < 0 || _delayJobs[index].IsCompleted;
        }

        public void StartJob(int jobId) {
            int index = _delayJobs.IndexOf(jobId);
            if (index < 0) return;

            _delayJobs[index] = _delayJobs[index].Start();
        }

        public void StopJob(int jobId) {
            int index = _delayJobs.IndexOf(jobId);
            if (index < 0) return;

            _delayJobs[index] = _delayJobs[index].Stop();
        }

        public void OnUpdate(float dt) {
            for (int i = _delayJobs.Count - 1; i >= 0; i--) {
                var delayJob = _delayJobs[i];

                if (delayJob.IsCompleted) {
                    _delayJobs.RemoveAt(i);
                    continue;
                }

                if (!delayJob.isUpdating) continue;

                delayJob = delayJob.AddToTimer(dt);

                if (delayJob.IsCompleted) {
                    _delayJobs.RemoveAt(i);
                    continue;
                }

                _delayJobs[i] = delayJob;
            }
        }

        private readonly struct DelayJobData {

            public bool IsCompleted => _timer >= _delay;
            public readonly bool isUpdating;

            private readonly float _delay;
            private readonly float _timer;

            public static DelayJobData Create(float delay) {
                return new DelayJobData(delay, 0f, false);
            }

            private DelayJobData(float delay, float timer, bool isUpdating) {
                _delay = delay;
                _timer = timer;
                this.isUpdating = isUpdating;
            }

            public DelayJobData AddToTimer(float value) {
                return new DelayJobData(_delay, _timer + value, isUpdating);
            }

            public DelayJobData Start() {
                return new DelayJobData(_delay, _timer, _timer < _delay);
            }

            public DelayJobData Stop() {
                return new DelayJobData(_delay, _timer, false);
            }
        }
    }

}
