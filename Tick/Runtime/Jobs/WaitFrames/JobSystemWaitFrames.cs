using System;
using MisterGames.Common.Data;
using MisterGames.Tick.Core;

namespace MisterGames.Tick.Jobs {

    [Serializable]
    internal sealed class JobSystemWaitFrames : IJobSystem, IUpdate {

        private readonly DictionaryList<int, JobData> _jobs = new DictionaryList<int, JobData>();
        private IJobIdFactory _jobIdFactory;

        public void Initialize(IJobIdFactory jobIdFactory) {
            _jobIdFactory = jobIdFactory;
        }

        public void DeInitialize() {
            _jobs.Clear();
        }

        public Job CreateJob(int frames) {
            var jobData = new JobData(frames);
            if (jobData.IsCompleted) return Jobs.Completed;

            int jobId = _jobIdFactory.CreateNewJobId();
            _jobs.Add(jobId, jobData);

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
            for (int i = _jobs.Count - 1; i >= 0; i--) {
                var job = _jobs.Values[i];

                if (job.IsCompleted) {
                    _jobs.RemoveAt(i);
                    continue;
                }

                if (!job.isUpdating) continue;

                _jobs.Values[i] = job.AddFramesCount(1);
            }
        }

        private readonly struct JobData {

            public bool IsCompleted => _frameCounter >= _frames;
            public readonly bool isUpdating;

            private readonly int _frames;
            private readonly int _frameCounter;

            public JobData(int frames, int frameCounter = 0, bool isUpdating = false) {
                _frames = frames;
                _frameCounter = frameCounter;
                this.isUpdating = isUpdating;
            }

            public JobData AddFramesCount(int frames) {
                return new JobData(_frames, _frameCounter + frames, isUpdating);
            }

            public JobData Start() {
                return new JobData(_frames, _frameCounter, _frameCounter < _frames);
            }

            public JobData Stop() {
                return new JobData(_frames, _frameCounter, false);
            }
        }
    }

}
