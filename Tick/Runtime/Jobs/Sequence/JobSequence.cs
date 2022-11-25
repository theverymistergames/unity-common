using System.Collections.Generic;
using MisterGames.Tick.Core;
using UnityEngine;

namespace MisterGames.Tick.Jobs {
    
    public sealed class JobSequence : IJob, IUpdate {

        public bool IsCompleted => _jobs.Count == 0;
        public float Progress => _jobs.Count == 0 ? 1f : _progress;

        private readonly Queue<IJob> _jobs = new Queue<IJob>();

        private int _totalJobs;
        private int _completeCount;
        private float _progress;
        private bool _isUpdating;

        public static JobSequence Create(params IJob[] jobs) {
            return new JobSequence(jobs);
        }

        private JobSequence() { }

        private JobSequence(IReadOnlyList<IJob> jobs) {
            for (int i = 0; i < jobs.Count; i++) {
                RequestAddJob(jobs[i]);
            }
            UpdateProgress();
        }

        public JobSequence Add(IJob job) {
            RequestAddJob(job);
            UpdateProgress();
            return this;
        }

        public JobSequence<R> Add<R>(IJob<R> job) {
            RequestAddJob(job);
            UpdateProgress();
            return new JobSequence<R>(this, job);
        }

        public JobSequence<R> Add<R>(IJob<R> job, out IJobReadOnly<R> resultJob) {
            resultJob = job;
            return Add(job);
        }

        public void Start() {
            StartJobsUntilUnableToComplete();
            UpdateProgress();
            _isUpdating = _jobs.Count > 0;
        }

        public void Stop() {
            _isUpdating = false;
            if (_jobs.TryPeek(out var job)) job.Stop();
        }

        void IUpdate.OnUpdate(float dt) {
            if (!_isUpdating) return;

            if (!_jobs.TryPeek(out var job)) {
                _isUpdating = false;
                _progress = 1f;
                return;
            }

            if (job.IsCompleted) {
                StartJobsUntilUnableToComplete();
                UpdateProgress();
                _isUpdating = _jobs.Count > 0;
                return;
            }

            if (job is IUpdate update) update.OnUpdate(dt);

            if (job.IsCompleted) {
                StartJobsUntilUnableToComplete();
                UpdateProgress();
                _isUpdating = _jobs.Count > 0;
            }
        }

        private void RequestAddJob(IJob job) {
            _totalJobs++;

            if (job.IsCompleted) _completeCount++;
            else _jobs.Enqueue(job);
        }

        private void StartJobsUntilUnableToComplete() {
            while (_jobs.TryPeek(out var job)) {
                if (!job.IsCompleted) job.Start();
                if (!job.IsCompleted) return;

                _completeCount++;
                _jobs.Dequeue();
            }
        }

        private void UpdateProgress() {
            if (!_jobs.TryPeek(out var job)) {
                _progress = 1f;
                return;
            }

            float jobProgress = job.IsCompleted ? 1f : job.Progress;
            _progress = Mathf.Clamp01((_completeCount + jobProgress) / _totalJobs);
        }
    }

    public sealed class JobSequence<R> : IJob<R>, IUpdate {

        public bool IsCompleted => _sequence.IsCompleted;
        public float Progress => _sequence.Progress;
        public R Result => _resultJob.Result;

        private readonly JobSequence _sequence;
        private IJobReadOnly<R> _resultJob;

        private JobSequence() { }

        internal JobSequence(JobSequence sequence, IJobReadOnly<R> resultJob) {
            _sequence = sequence;
            _resultJob = resultJob;
        }

        public JobSequence<R> Add(IJob job) {
            _sequence.Add(job);
            return this;
        }

        public JobSequence<R> Add(IJob<R> job) {
            _sequence.Add(job);
            _resultJob = job;
            return this;
        }

        public JobSequence<T> Add<T>(IJob<T> job) {
            _sequence.Add(job);
            return new JobSequence<T>(_sequence, job);
        }

        public JobSequence<R> Add(IJob<R> job, out IJobReadOnly<R> resultJob) {
            resultJob = job;
            return Add(job);
        }

        public JobSequence<T> Add<T>(IJob<T> job, out IJobReadOnly<T> resultJob) {
            resultJob = job;
            return Add(job);
        }

        public void Start() {
            _sequence.Start();
        }

        public void Stop() {
            _sequence.Stop();
        }

        void IUpdate.OnUpdate(float dt) {
            if (_sequence is IUpdate update) update.OnUpdate(dt);
        }
    }
    
}
