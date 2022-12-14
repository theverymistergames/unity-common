﻿using System;
using MisterGames.Common.Data;
using MisterGames.Tick.Core;

namespace MisterGames.Tick.Jobs {

    [Serializable]
    public sealed class JobSystemEachFrame : IJobSystem, IUpdate {

        private readonly DictionaryList<int, JobData> _jobs = new DictionaryList<int, JobData>();
        private IJobIdFactory _jobIdFactory;

        public void Initialize(IJobIdFactory jobIdFactory) {
            _jobIdFactory = jobIdFactory;
        }

        public void DeInitialize() {
            _jobs.Clear();
        }

        public Job CreateJob(Action<float> action) {
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

                if (!job.isUpdating) continue;

                job.action.Invoke(dt);
            }
        }

        private readonly struct JobData {

            public static readonly JobData Completed = new JobData(null, false, true);

            public readonly bool isUpdating;
            public readonly bool isCompleted;
            public readonly Action<float> action;

            public JobData(Action<float> action, bool isUpdating = false, bool isCompleted = false) {
                this.action = action;
                this.isUpdating = isUpdating;
                this.isCompleted = isCompleted;
            }

            public JobData Start() {
                return new JobData(action, !isCompleted, isCompleted);
            }

            public JobData Stop() {
                return new JobData(action, false, isCompleted);
            }
        }
    }

}
