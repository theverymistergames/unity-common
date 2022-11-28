using System;
using MisterGames.Tick.Core;

namespace MisterGames.Tick.Jobs.Structs {

    [Serializable]
    public sealed class JobSystemDelay : IJobSystem<float>, IUpdate {

        private JobsContainer<JobData> _jobsContainer;

        public void Initialize(IJobIdFactory jobFactory) {
            _jobsContainer = new JobsContainer<JobData>(jobFactory);
        }

        public void DeInitialize() {
            _jobsContainer.Clear();
        }

        public int CreateJob(float data) {
            return data > 0f ? _jobsContainer.AddNewJobData(new JobData(delay: data)) : -1;
        }

        public bool IsJobCompleted(int jobId) {
            int index = _jobsContainer.IndexOf(jobId);
            if (index < 0) return true;

            var data = _jobsContainer[index];
            return data.timer >= data.delay;
        }

        public void StartJob(int jobId) {
            int index = _jobsContainer.IndexOf(jobId);
            if (index < 0) return;

            var data = _jobsContainer[index];
            if (data.timer < data.delay) {
                _jobsContainer[index] = data.Start();
                return;
            }

            _jobsContainer.RemoveAt(index);
        }

        public void StopJob(int jobId) {
            int index = _jobsContainer.IndexOf(jobId);
            if (index < 0) return;

            var data = _jobsContainer[index];
            _jobsContainer[index] = data.Stop();
        }

        public void OnUpdate(float dt) {
            for (int i = _jobsContainer.Count - 1; i >= 0; i--) {
                var data = _jobsContainer[i];
                if (!data.isUpdating) continue;

                float timer = data.timer + dt;
                if (timer < data.delay) {
                    _jobsContainer[i] = data.SetTimer(timer);
                    continue;
                }

                _jobsContainer.RemoveAt(i);
            }
        }

        private readonly struct JobData {

            public readonly float delay;
            public readonly float timer;
            public readonly bool isUpdating;

            public JobData(float delay, float timer = 0f, bool isUpdating = false) {
                this.delay = delay;
                this.timer = timer;
                this.isUpdating = isUpdating;
            }

            public JobData Start() {
                return new JobData(delay, timer, timer < delay);
            }

            public JobData Stop() {
                return new JobData(delay, timer, false);
            }

            public JobData SetTimer(float value) {
                return new JobData(delay, value, isUpdating);
            }
        }
    }

}
