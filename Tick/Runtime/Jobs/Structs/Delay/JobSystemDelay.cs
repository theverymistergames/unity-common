using System;
using MisterGames.Tick.Core;

namespace MisterGames.Tick.Jobs.Structs {

    [Serializable]
    internal sealed class JobSystemDelay : IJobSystem<float>, IUpdate {

        private readonly JobsDataContainer<DelayJobData> _delayJobs = new JobsDataContainer<DelayJobData>();

        private ITimeSource _timeSource;
        private IJobIdFactory _jobIdFactory;
        private bool _isUpdating;

        public void Initialize(ITimeSource timeSource, IJobIdFactory jobIdFactory) {
            _timeSource = timeSource;
            _jobIdFactory = jobIdFactory;
        }

        public void DeInitialize() {
            _delayJobs.Clear();
            _timeSource.Unsubscribe(this);
        }

        public int CreateJob(float data) {
            var jobData = DelayJobData.Create(data);
            if (jobData.IsCompleted()) return -1;

            int jobId = _jobIdFactory.CreateNewJobId();
            _delayJobs.Add(jobId, jobData);

            return jobId;
        }

        public bool IsJobCompleted(int jobId) {
            int index = _delayJobs.IndexOf(jobId);
            return index < 0 || _delayJobs[index].IsCompleted();
        }

        public void StartJob(int jobId) {
            int index = _delayJobs.IndexOf(jobId);
            if (index < 0) return;

            _delayJobs[index] = _delayJobs[index].Start();

            if (!_isUpdating) {
                _isUpdating = true;
                _timeSource.Subscribe(this);
            }
        }

        public void StopJob(int jobId) {
            int index = _delayJobs.IndexOf(jobId);
            if (index < 0) return;

            _delayJobs[index] = _delayJobs[index].Stop();

            if (_isUpdating && _delayJobs.Count == 1) {
                _timeSource.Unsubscribe(this);
                _isUpdating = false;
            }
        }

        public void OnUpdate(float dt) {
            for (int i = _delayJobs.Count - 1; i >= 0; i--) {
                var data = _delayJobs[i];

                if (data.IsCompleted()) {
                    _delayJobs.RemoveAt(i);
                    continue;
                }

                if (!data.isUpdating) continue;

                data = data.AddToTimer(dt);

                if (data.IsCompleted()) {
                    _delayJobs.RemoveAt(i);
                    continue;
                }

                _delayJobs[i] = data;
            }

            if (_delayJobs.Count == 0) {
                _timeSource.Unsubscribe(this);
                _isUpdating = false;
            }
        }

        private readonly struct DelayJobData {

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

            public bool IsCompleted() {
                return _timer >= _delay;
            }

            public DelayJobData Start() {
                return new DelayJobData(_delay, _timer, _timer < _delay);
            }

            public DelayJobData Stop() {
                return new DelayJobData(_delay, _timer, false);
            }

            public DelayJobData AddToTimer(float value) {
                return new DelayJobData(_delay, _timer + value, isUpdating);
            }
        }
    }

}
