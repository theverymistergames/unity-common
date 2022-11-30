using System;
using MisterGames.Tick.Core;

namespace MisterGames.Tick.Jobs.Structs {

    [Serializable]
    public sealed class JobSystemSequence : IJobSystem<float>, IUpdate {

        private readonly JobsDataContainer<JobData> _jobsData = new JobsDataContainer<JobData>();

        private ITimeSource _timeSource;
        private IJobIdFactory _jobIdFactory;
        private bool _isUpdating;

        public void Initialize(ITimeSource timeSource, IJobIdFactory jobIdFactory) {
            _timeSource = timeSource;
            _jobIdFactory = jobIdFactory;
        }

        public void DeInitialize() {
            _jobsData.Clear();
            _timeSource.Unsubscribe(this);
        }

        public int CreateJob(float data) {
            var jobData = new JobData(data);
            if (jobData.IsCompleted()) return -1;

            int jobId = _jobIdFactory.CreateNewJobId();
            _jobsData.Add(jobId, jobData);

            return jobId;
        }

        public bool IsJobCompleted(int jobId) {
            int index = _jobsData.IndexOf(jobId);
            return index < 0 || _jobsData[index].IsCompleted();
        }

        public void StartJob(int jobId) {
            int index = _jobsData.IndexOf(jobId);
            if (index < 0) return;

            _jobsData[index] = _jobsData[index].Start();

            if (!_isUpdating) {
                _isUpdating = true;
                _timeSource.Subscribe(this);
            }
        }

        public void StopJob(int jobId) {
            int index = _jobsData.IndexOf(jobId);
            if (index < 0) return;

            _jobsData[index] = _jobsData[index].Stop();

            if (_isUpdating && _jobsData.Count == 1) {
                _timeSource.Unsubscribe(this);
                _isUpdating = false;
            }
        }

        public void OnUpdate(float dt) {
            for (int i = _jobsData.Count - 1; i >= 0; i--) {
                var data = _jobsData[i];

                if (data.IsCompleted()) {
                    _jobsData.RemoveAt(i);
                    continue;
                }

                if (!data.isUpdating) continue;

                data = data.AddToTimer(dt);

                if (data.IsCompleted()) {
                    _jobsData.RemoveAt(i);
                    continue;
                }

                _jobsData[i] = data;
            }

            if (_jobsData.Count == 0) {
                _timeSource.Unsubscribe(this);
                _isUpdating = false;
            }
        }

        private readonly struct JobData {

            public readonly bool isUpdating;
            private readonly float _delay;
            private readonly float _timer;

            public JobData(float delay, float timer = 0f, bool isUpdating = false) {
                _delay = delay;
                _timer = timer;
                this.isUpdating = isUpdating;
            }

            public bool IsCompleted() {
                return _timer >= _delay;
            }

            public JobData Start() {
                return new JobData(_delay, _timer, _timer < _delay);
            }

            public JobData Stop() {
                return new JobData(_delay, _timer, false);
            }

            public JobData AddToTimer(float value) {
                return new JobData(_delay, _timer + value, isUpdating);
            }
        }
    }

}
