using System.Collections.Generic;
using MisterGames.Tick.Core;

namespace MisterGames.Tick.Jobs {
    
    public sealed class JobSequence : IJob, IUpdate {

        public bool IsCompleted => _jobs.Count == 0;

        private readonly Queue<IJob> _jobs;

        private bool _isUpdating;

        public static JobSequence Create(params IJob[] jobs) {
            return new JobSequence(jobs);
        }

        private JobSequence() { }

        private JobSequence(IEnumerable<IJob> jobs) {
            _jobs = new Queue<IJob>(jobs);
        }

        public JobSequence Add(IJob job) {
            if (!job.IsCompleted) _jobs.Enqueue(job);
            return this;
        }

        public JobSequence<R> Add<R>(IJob<R> job) {
            if (!job.IsCompleted) _jobs.Enqueue(job);
            return new JobSequence<R>(this, job);
        }

        public JobSequence<R> Add<R>(IJob<R> job, out IJobReadOnly<R> resultJob) {
            resultJob = job;
            return Add(job);
        }

        public void Start() {
            StartJobsUntilUnableToComplete();
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
                return;
            }

            if (job.IsCompleted) {
                StartJobsUntilUnableToComplete();
                return;
            }

            if (job is IUpdate update) update.OnUpdate(dt);

            if (job.IsCompleted) {
                StartJobsUntilUnableToComplete();
            }
        }

        private void StartJobsUntilUnableToComplete() {
            while (_jobs.TryPeek(out var job)) {
                if (!job.IsCompleted) job.Start();
                if (!job.IsCompleted) return;

                _jobs.Dequeue();
            }
        }
    }

    public sealed class JobSequence<R> : IJob<R>, IUpdate {

        public bool IsCompleted => _sequence.IsCompleted;
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
