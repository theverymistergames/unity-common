using System;
using MisterGames.Tick.Core;

namespace MisterGames.Tick.Jobs.Structs {

    [Serializable]
    internal sealed class JobSystemSequence : IJobSystem, IUpdate {

        private readonly JobsDataContainer<JobSequenceNode> _jobSequenceNodes = new JobsDataContainer<JobSequenceNode>();

        private ITimeSource _timeSource;
        private IJobIdFactory _jobIdFactory;
        private bool _isUpdating;

        public void Initialize(ITimeSource timeSource, IJobIdFactory jobIdFactory) {
            _timeSource = timeSource;
            _jobIdFactory = jobIdFactory;
        }

        public void DeInitialize() {
            _jobSequenceNodes.Clear();
            _timeSource.Unsubscribe(this);
        }

        public int CreateJob() {
            return _jobIdFactory.CreateNewJobId();
        }

        public void AddJobIntoSequence(int sequenceJobId, Job job) {
            int lastIndex = _jobSequenceNodes.LastIndexOf(sequenceJobId);
            var waitJob = lastIndex < 0 ? Jobs.Completed : _jobSequenceNodes[lastIndex].nextJob;
            _jobSequenceNodes.Add(sequenceJobId, JobSequenceNode.Create(waitJob, job));
        }

        public bool IsJobCompleted(int jobId) {
            int lastIndex = _jobSequenceNodes.LastIndexOf(jobId);
            return lastIndex < 0 || _jobSequenceNodes[lastIndex].isCompleted;
        }

        public void StartJob(int jobId) {
            int firstIndex = _jobSequenceNodes.IndexOf(jobId);
            if (firstIndex < 0) return;

            int lastIndex = _jobSequenceNodes.LastIndexOf(jobId);
            for (int i = firstIndex; i <= lastIndex; i++) {
                if (jobId != _jobSequenceNodes.Keys[i]) continue;

                var node = _jobSequenceNodes[i];
                if (node.isCompleted) continue;

                if (node.waitJob.IsCompleted) {
                    node.nextJob.Start();
                    _jobSequenceNodes[i] = JobSequenceNode.Completed;
                    continue;
                }

                _jobSequenceNodes[i] = node.Start();
                break;
            }

            if (!_isUpdating) {
                _isUpdating = true;
                _timeSource.Subscribe(this);
            }
        }

        public void StopJob(int jobId) {
            int firstIndex = _jobSequenceNodes.IndexOf(jobId);
            if (firstIndex < 0) return;

            int lastIndex = _jobSequenceNodes.LastIndexOf(jobId);
            for (int i = firstIndex; i <= lastIndex; i++) {
                if (jobId != _jobSequenceNodes.Keys[i]) continue;

                var node = _jobSequenceNodes[i];
                
                if (node.waitJob.IsCompleted) {
                    if (node.nextJob.IsCompleted) continue;
                    node.nextJob.Stop();
                }
                else node.waitJob.Stop();

                _jobSequenceNodes[i] = node.Stop();
                break;
            }

            if (_isUpdating && _jobSequenceNodes.Count == 1) {
                _timeSource.Unsubscribe(this);
                _isUpdating = false;
            }
        }

        public void OnUpdate(float dt) {
            for (int i = _jobSequenceNodes.Count - 1; i >= 0; i--) {
                var node = _jobSequenceNodes[i];

                if (node.isCompleted) {
                    _jobSequenceNodes.RemoveAt(i);
                    continue;
                }

                if (!node.isUpdating || !node.waitJob.IsCompleted) continue;

                node.nextJob.Start();
                _jobSequenceNodes.RemoveAt(i);
            }

            if (_jobSequenceNodes.Count == 0) {
                _timeSource.Unsubscribe(this);
                _isUpdating = false;
            }
        }

        private readonly struct JobSequenceNode {

            public readonly Job waitJob;
            public readonly Job nextJob;

            public readonly bool isUpdating;
            public readonly bool isCompleted;

            public static readonly JobSequenceNode Completed =
                new JobSequenceNode(default, default, false, true);

            public static JobSequenceNode Create(Job waitJob, Job nextJob) =>
                new JobSequenceNode(waitJob, nextJob, false, false);

            private JobSequenceNode(Job waitJob, Job nextJob, bool isUpdating, bool isCompleted) {
                this.waitJob = waitJob;
                this.nextJob = nextJob;
                this.isUpdating = isUpdating;
                this.isCompleted = isCompleted;
            }

            public JobSequenceNode Start() {
                return new JobSequenceNode(waitJob, nextJob, !isCompleted, isCompleted);
            }

            public JobSequenceNode Stop() {
                return new JobSequenceNode(waitJob, nextJob, false, isCompleted);
            }
        }
    }

}
