using System.Collections.Generic;
using MisterGames.Tick.Core;

namespace MisterGames.Tick.Jobs {
    
    internal sealed class JobSequence : IJob, IUpdate {

        public bool IsCompleted => CheckCompleted();

        private readonly List<IJob> _jobs;

        private bool _isUpdating;
        private int _currentJobIndex;

        public JobSequence(List<IJob> jobs) {
            _jobs = jobs;
        }

        public void Start() {
            if (_currentJobIndex >= _jobs.Count) {
                _isUpdating = false;
                return;
            }

            StartJobsUntilUnableToComplete(_currentJobIndex);
        }

        public void Stop() {
            _isUpdating = false;
            if (_currentJobIndex < _jobs.Count) _jobs[_currentJobIndex].Stop();
        }

        public void Reset() {
            _currentJobIndex = 0;
            _isUpdating = false;
        }

        void IUpdate.OnUpdate(float dt) {
            if (!_isUpdating) return;

            if (_currentJobIndex >= _jobs.Count) {
                _isUpdating = false;
                return;
            }

            var job = _jobs[_currentJobIndex];

            if (job.IsCompleted) {
                StartJobsUntilUnableToComplete(_currentJobIndex + 1);
                return;
            }

            if (job is IUpdate update) update.OnUpdate(dt);

            if (job.IsCompleted) {
                StartJobsUntilUnableToComplete(_currentJobIndex + 1);
            }
        }

        private void StartJobsUntilUnableToComplete(int startIndex) {
            int jobsCount = _jobs.Count;

            for (int i = startIndex; i < jobsCount; i++) {
                var job = _jobs[i];
                job.Start();

                if (job.IsCompleted) continue;

                _currentJobIndex = i;
                return;
            }

            _currentJobIndex = jobsCount;
        }

        private bool CheckCompleted() {
            int count = _jobs.Count;
            return count == 0 || _currentJobIndex >= count;
        }
    }
    
}
