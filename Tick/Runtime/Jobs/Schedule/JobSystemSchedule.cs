using System;
using MisterGames.Common.Data;
using MisterGames.Tick.Core;

namespace MisterGames.Tick.Jobs {

    [Serializable]
    public sealed class JobSystemSchedule : IJobSystem, IUpdate {

        private readonly DictionaryList<int, JobData> _jobs = new DictionaryList<int, JobData>();
        private IJobIdFactory _jobIdFactory;

        public void Initialize(IJobIdFactory jobIdFactory) {
            _jobIdFactory = jobIdFactory;
        }

        public void DeInitialize() {
            _jobs.Clear();
        }

        public Job CreateJob(Action action, float period, int maxTimes = -1) {
            if (maxTimes == 0) return Jobs.Completed;

            int jobId = _jobIdFactory.CreateNewJobId();
            _jobs.Add(jobId, new JobData(action, period, maxTimes));

            return new Job(jobId, this);
        }

        public bool IsJobCompleted(int jobId) {
            int index = _jobs.Keys.IndexOf(jobId);
            return index < 0 || _jobs.Values[index].isCompleted;
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

                if (!job.isUpdating) continue;

                _jobs.Values[i] = job.AddToTimerAndTryInvokeScheduleAction(dt);
            }
        }

        private readonly struct JobData {

            public static readonly JobData Completed
                = new JobData(null, 0f, 0, 0f, 0, false, true);

            public readonly bool isUpdating;
            public readonly bool isCompleted;

            private readonly int _maxTimes;
            private readonly int _timesCount;

            private readonly float _period;
            private readonly float _periodTimer;

            private readonly Action _action;

            public JobData(
                Action action,
                float period,
                int maxTimes,
                float timer = 0f,
                int timesCount = 0,
                bool isUpdating = false,
                bool isCompleted = false
            ) {
                _action = action;

                _maxTimes = maxTimes;
                _timesCount = timesCount;

                _period = period;
                _periodTimer = timer;

                this.isUpdating = isUpdating;
                this.isCompleted = isCompleted;
            }

            public JobData AddToTimerAndTryInvokeScheduleAction(float dt) {
                int nextTimesCount = _timesCount;
                float nextTimer = _periodTimer + dt;
                bool canContinue = _maxTimes < 0 || _timesCount < _maxTimes;

                if (canContinue && nextTimer >= _period) {
                    nextTimer = 0f;
                    nextTimesCount++;
                    _action.Invoke();
                }

                canContinue = _maxTimes < 0 || nextTimesCount < _maxTimes;

                return new JobData(_action, _period, _maxTimes, nextTimer, nextTimesCount, canContinue, !canContinue);
            }

            public JobData Start() {
                return new JobData(_action, _period, _maxTimes, _periodTimer, _timesCount, !isCompleted, isCompleted);
            }

            public JobData Stop() {
                return new JobData(_action, _period, _maxTimes, _periodTimer, _timesCount, false, isCompleted);
            }
        }
    }

}
