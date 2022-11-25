using System;
using System.Collections.Generic;
using UnityEngine;

namespace MisterGames.Tick.Jobs {
    
    public sealed class JobObserver : IJobReadOnly {

        public bool IsCompleted {
            get {
                RemoveCompletedJobsAndUpdateProgress();
                return _progress >= 1f;
            }
        }

        public float Progress {
            get {
                RemoveCompletedJobsAndUpdateProgress();
                return _progress;
            }
        }

        private readonly List<IJobReadOnly> _jobs = new List<IJobReadOnly>();
        private readonly ProgressMode _progressMode;

        private int _completeCount;
        private int _totalCount;
        private float _progress;

        public enum ProgressMode {
            ProgressOfNotCompleted,
            TotalObservedProgress,
        }

        public JobObserver(ProgressMode progressMode = ProgressMode.ProgressOfNotCompleted) {
            _progressMode = progressMode;
        }

        public void Observe(IJobReadOnly job) {
            _jobs.Add(job);
            _totalCount++;
        }

        public void ObserveAll(params IJobReadOnly[] jobs) {
            _jobs.AddRange(jobs);
            _totalCount += jobs.Length;
        }

        public void Clear() {
            _jobs.Clear();
            _progress = 1f;
            _totalCount = 0;
            _completeCount = 0;
        }

        private void RemoveCompletedJobsAndUpdateProgress() {
            if (_totalCount <= 0) {
                _progress = 1f;
                return;
            }

            float progressSumOfNotCompleted = 0f;
            for (int i = _jobs.Count - 1; i >= 0; i--) {
                var job = _jobs[i];

                if (job.IsCompleted) {
                    _jobs.RemoveAt(i);
                    _completeCount++;
                    continue;
                }

                progressSumOfNotCompleted += job.Progress;
            }

            switch (_progressMode) {
                case ProgressMode.ProgressOfNotCompleted:
                    int notCompletedCount = _jobs.Count;
                    _progress = notCompletedCount == 0 ? 1f : Mathf.Clamp01(progressSumOfNotCompleted / notCompletedCount);
                    break;

                case ProgressMode.TotalObservedProgress:
                    _progress = Mathf.Clamp01((_completeCount + progressSumOfNotCompleted) / _totalCount);
                    break;

                default:
                    throw new NotImplementedException($"ProgressMode {_progressMode} is not implemented for {nameof(JobObserver)}");
            }
        }
    }
    
}
