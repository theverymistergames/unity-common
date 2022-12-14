﻿using System;
using MisterGames.Common.Data;
using MisterGames.Tick.Core;

namespace MisterGames.Tick.Jobs {

    [Serializable]
    public sealed class JobSystemDelay : IJobSystem, IUpdate {

        private readonly DictionaryList<int, JobData> _jobs = new DictionaryList<int, JobData>();
        private IJobIdFactory _jobIdFactory;

        public void Initialize(IJobIdFactory jobIdFactory) {
            _jobIdFactory = jobIdFactory;
        }

        public void DeInitialize() {
            _jobs.Clear();
        }

        public Job CreateJob(float delay) {
            if (delay <= 0f) return Jobs.Completed;

            int jobId = _jobIdFactory.CreateNewJobId();
            _jobs.Add(jobId, new JobData(delay));

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

        public void DisposeJob(int jobId) {
            int index = _jobs.Keys.IndexOf(jobId);
            if (index >= 0) _jobs.Values[index] = default;
        }

        public void OnUpdate(float dt) {
            int count = _jobs.Count;
            for (int i = 0; i < count; i++) {
                var job = _jobs.Values[i];

                if (job.IsCompleted) {
                    _jobs.RemoveAt(i--);
                    count--;
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
