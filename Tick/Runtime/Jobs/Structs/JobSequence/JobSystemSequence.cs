using System;
using MisterGames.Tick.Core;

namespace MisterGames.Tick.Jobs.Structs {

    [Serializable]
    internal sealed class JobSystemSequence : IJobSystem, IUpdate {

        private readonly JobsDataContainer<JobSequenceNode> _nodes = new JobsDataContainer<JobSequenceNode>();
        private IJobIdFactory _jobIdFactory;

        public void Initialize(IJobIdFactory jobIdFactory) {
            _jobIdFactory = jobIdFactory;
        }

        public void DeInitialize() {
            _nodes.Clear();
        }

        public int CreateJob() {
            return _jobIdFactory.CreateNewJobId();
        }

        public void AddJobIntoSequence(int sequenceJobId, Job job) {
            int lastIndex = _nodes.LastIndexOf(sequenceJobId);
            var waitJob = lastIndex < 0 ? Jobs.Completed : _nodes[lastIndex].nextJob;
            _nodes.Add(sequenceJobId, JobSequenceNode.Create(waitJob, job));
        }

        public bool IsJobCompleted(int jobId) {
            int lastIndex = _nodes.LastIndexOf(jobId);
            return lastIndex < 0 || _nodes[lastIndex].isCompleted;
        }

        public void StartJob(int jobId) {
            int firstIndex = _nodes.IndexOf(jobId);
            if (firstIndex < 0) return;

            int lastIndex = _nodes.LastIndexOf(jobId);
            for (int i = firstIndex; i <= lastIndex; i++) {
                if (jobId != _nodes.Keys[i]) continue;

                var node = _nodes[i];
                if (node.isCompleted) continue;

                if (node.waitJob.IsCompleted) {
                    node.nextJob.Start();
                    _nodes[i] = JobSequenceNode.Completed;
                    continue;
                }

                _nodes[i] = node.Start();
                return;
            }
        }

        public void StopJob(int jobId) {
            int firstIndex = _nodes.IndexOf(jobId);
            if (firstIndex < 0) return;

            int lastIndex = _nodes.LastIndexOf(jobId);
            for (int i = firstIndex; i <= lastIndex; i++) {
                if (jobId != _nodes.Keys[i]) continue;

                var node = _nodes[i];
                
                if (node.waitJob.IsCompleted) {
                    if (node.nextJob.IsCompleted) continue;
                    node.nextJob.Stop();
                }
                else node.waitJob.Stop();

                _nodes[i] = node.Stop();
                return;
            }
        }

        public void OnUpdate(float dt) {
            for (int i = _nodes.Count - 1; i >= 0; i--) {
                var node = _nodes[i];

                if (node.isCompleted) {
                    _nodes.RemoveAt(i);
                    continue;
                }

                if (!node.isUpdating || !node.waitJob.IsCompleted) continue;

                node.nextJob.Start();
                _nodes.RemoveAt(i);
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
