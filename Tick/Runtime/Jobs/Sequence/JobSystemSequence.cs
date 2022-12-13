﻿using System;
using MisterGames.Common.Data;
using MisterGames.Tick.Core;

namespace MisterGames.Tick.Jobs {

    [Serializable]
    public sealed class JobSystemSequence : IJobSystem, IUpdate {

        private readonly DictionaryList<int, JobSequenceNode> _nodes = new DictionaryList<int, JobSequenceNode>();
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
            if (job.IsCompleted) return;

            int lastIndex = _nodes.Keys.LastIndexOf(sequenceJobId);
            if (lastIndex < 0) {
                _nodes.Add(sequenceJobId, new JobSequenceNode(Jobs.Completed, job));
                _nodes.Add(sequenceJobId, new JobSequenceNode(job, default));
                return;
            }

            _nodes.Values[lastIndex] = new JobSequenceNode(_nodes.Values[lastIndex].waitJob, job);
            _nodes.Add(sequenceJobId, new JobSequenceNode(job, default));
        }

        public bool IsJobCompleted(int jobId) {
            int lastIndex = _nodes.Keys.LastIndexOf(jobId);
            return lastIndex < 0 || _nodes.Values[lastIndex].isCompleted;
        }

        public void StartJob(int jobId) {
            int firstIndex = _nodes.Keys.IndexOf(jobId);
            if (firstIndex < 0) return;

            int lastIndex = _nodes.Keys.LastIndexOf(jobId);
            for (int i = firstIndex; i <= lastIndex; i++) {
                if (jobId != _nodes.Keys[i]) continue;

                var node = _nodes.Values[i];
                if (node.isCompleted) continue;

                if (node.waitJob.IsCompleted) {
                    node.nextJob.Start();
                    _nodes.Values[i] = JobSequenceNode.Completed;
                    continue;
                }

                node.waitJob.Start();
                _nodes.Values[i] = node.Start();
                return;
            }
        }

        public void StopJob(int jobId) {
            int firstIndex = _nodes.Keys.IndexOf(jobId);
            if (firstIndex < 0) return;

            int lastIndex = _nodes.Keys.LastIndexOf(jobId);
            for (int i = firstIndex; i <= lastIndex; i++) {
                if (jobId != _nodes.Keys[i]) continue;

                var node = _nodes.Values[i];

                if (node.waitJob.IsCompleted) {
                    if (node.nextJob.IsCompleted) continue;
                    node.nextJob.Stop();
                }
                else node.waitJob.Stop();

                _nodes.Values[i] = node.Stop();
                return;
            }
        }

        public void DisposeJob(int jobId) {
            int firstIndex = _nodes.Keys.IndexOf(jobId);
            if (firstIndex < 0) return;

            int lastIndex = _nodes.Keys.LastIndexOf(jobId);
            for (int i = firstIndex; i <= lastIndex; i++) {
                if (jobId != _nodes.Keys[i]) continue;
                _nodes.Values[i] = JobSequenceNode.Completed;
            }
        }

        public void OnUpdate(float dt) {
            for (int i = _nodes.Count - 1; i >= 0; i--) {
                var node = _nodes.Values[i];
                if (node.isCompleted) {
                    _nodes.RemoveAt(i);
                    continue;
                }

                if (!node.isUpdating || !node.waitJob.IsCompleted) continue;

                int sequenceJobId = _nodes.Keys[i];
                node.nextJob.Start();
                _nodes.RemoveAt(i);

                int nextNodeIndex = _nodes.Keys.IndexOf(sequenceJobId, i);
                if (nextNodeIndex < 0) continue;

                _nodes.Values[nextNodeIndex] = _nodes.Values[nextNodeIndex].Start();
            }
        }

        private readonly struct JobSequenceNode {

            public readonly Job waitJob;
            public readonly Job nextJob;

            public readonly bool isUpdating;
            public readonly bool isCompleted;

            public static readonly JobSequenceNode Completed =
                new JobSequenceNode(default, default, false, true);

            public JobSequenceNode(Job waitJob, Job nextJob, bool isUpdating = false, bool isCompleted = false) {
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
