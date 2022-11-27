using System;
using System.Collections.Generic;
using MisterGames.Tick.Core;
using UnityEngine;

namespace MisterGames.Tick.Jobs.Structs {

    public static class Jobs {

        private static IJobBuilder<float> _delayBuilder;

        public static Job Delay(float delay) {
            return _delayBuilder.Create(delay);
        }

    }

    public class DelayJobSystem : IJobSystem<float>, IUpdate {

        private readonly List<Job> _jobs = new List<Job>();
        private readonly List<JobData> _jobDataList = new List<JobData>();

        private struct JobData {
            public float delay;
            public float timer;
            public bool isUpdating;
        }

        public Job Create(float data) {
            var job = new Job();

            var jobData = new JobData {
                delay = data,
                timer = 0f,
                isUpdating = false,
            };

            _jobDataList.Add(jobData);
            _jobs.Add(job);

            return job;
        }

        public bool IsJobCompleted(Job job) {
            return _jobs.IndexOf(job) < 0;
        }

        public float GetJobProgress(Job job) {
            int index = _jobs.IndexOf(job);
            if (index < 0) return 1f;

            var jobData = _jobDataList[index];
            return jobData.delay <= 0f ? 1f : Mathf.Clamp01(jobData.timer / jobData.delay);
        }

        public void StartJob(Job job) {
            int index = _jobs.IndexOf(job);
            if (index < 0) return;

            var jobData = _jobDataList[index];
            jobData.isUpdating = jobData.timer < jobData.delay;

            _jobDataList[index] = jobData;
        }

        public void StopJob(Job job) {
            int index = _jobs.IndexOf(job);
            if (index < 0) return;

            var jobData = _jobDataList[index];
            jobData.isUpdating = false;

            _jobDataList[index] = jobData;
        }

        public void OnUpdate(float dt) {
            for (int i = _jobDataList.Count - 1; i >= 0; i--) {
                var jobData = _jobDataList[i];
                if (!jobData.isUpdating) continue;

                jobData.timer += dt;
                jobData.isUpdating = jobData.timer < jobData.delay;

                _jobDataList[i] = jobData;
            }
        }
    }

    public class JobDelayBuilder : IJobBuilder<float> {

        private readonly DelayJobSystem _delayJobSystem;

        public JobDelayBuilder(DelayJobSystem delayJobSystem) {
            _delayJobSystem = delayJobSystem;
        }

        public Job Create(float data) {
            return _delayJobSystem.Create(data);
        }
    }

    public readonly struct Job { }

    public interface IJobSystem {
        bool IsJobCompleted(Job job);
        float GetJobProgress(Job job);

        void StartJob(Job job);
        void StopJob(Job job);
    }

    public interface IJobSystem<in T> : IJobSystem {
        Job Create(T data);
    }

    public interface IJobSystemRunner : IJobSystem, IUpdate {

    }

    public interface IJobBuilder<in T> {
        Job Create(T data);
    }

    public interface IJobStorage {
        void AddJob(Job job);
        void RemoveJob(Job job);
        IReadOnlyList<Job> GetJobs();
    }

    public class JobSystemRunner : IJobSystemRunner {

        private readonly IJobStorage[] _storages;
        private readonly IJobSystem[] _systems;
        private readonly Dictionary<Type, int> _systemTypeToSystemIndexMap = new Dictionary<Type, int>();
        private readonly Dictionary<Job, int> _jobToSystemIndexMap = new Dictionary<Job, int>();

        public Job CreateJob() {

        }

        public void StartJob(Job job) {
            _systems[systemIndex].StartJob(job);
        }

        public void StopJob(Job job) {

        }

        public void OnUpdate(float dt) {
            for (int i = 0; i < _systems.Length; i++) {
                if (_systems[i] is IUpdate update) update.OnUpdate(dt);
            }
        }
    }
}
