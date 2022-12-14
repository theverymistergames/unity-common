using System;
using MisterGames.Common.Data;
using MisterGames.Tick.Core;

namespace MisterGames.Tick.Jobs {

    [Serializable]
    public sealed class JobSystemEachFrameWhile : IJobSystem, IUpdate {

        private readonly DictionaryList<int, JobData> _jobs = new DictionaryList<int, JobData>();
        private IJobIdFactory _jobIdFactory;

        public void Initialize(IJobIdFactory jobIdFactory) {
            _jobIdFactory = jobIdFactory;
        }

        public void DeInitialize() {
            _jobs.Clear();
        }

        public Job CreateJob(Func<float, bool> action, int maxFrames = -1) {
            if (maxFrames == 0) return Jobs.Completed;

            int jobId = _jobIdFactory.CreateNewJobId();
            _jobs.Add(jobId, new JobData(action, maxFrames));

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

                _jobs.Values[i] = job.TryInvokeNextFrameActionWhile(dt);
            }
        }

        private readonly struct JobData {

            public static readonly JobData Completed
                = new JobData(null, 0, 0, false, true);

            public readonly bool isUpdating;
            public readonly bool isCompleted;

            private readonly int _maxFrames;
            private readonly int _frameCount;
            private readonly Func<float, bool> _actionWhile;

            public JobData(Func<float, bool> actionWhile, int maxFrames, int frameCount = 0, bool isUpdating = false, bool isCompleted = false) {
                _actionWhile = actionWhile;
                _maxFrames = maxFrames;
                _frameCount = frameCount;

                this.isUpdating = isUpdating;
                this.isCompleted = isCompleted;
            }

            public JobData TryInvokeNextFrameActionWhile(float dt) {
                int nextFrameCount = _frameCount + 1;

                bool canContinue = _maxFrames < 0
                    ? _actionWhile.Invoke(dt)
                    : _frameCount < _maxFrames && _actionWhile.Invoke(dt) && nextFrameCount < _maxFrames;

                return new JobData(_actionWhile, _maxFrames, nextFrameCount, canContinue, !canContinue);
            }

            public JobData Start() {
                return new JobData(_actionWhile, _maxFrames, _frameCount, !isCompleted, isCompleted);
            }

            public JobData Stop() {
                return new JobData(_actionWhile, _maxFrames, _frameCount, false, isCompleted);
            }
        }
    }

}
