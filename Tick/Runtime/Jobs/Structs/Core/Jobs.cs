using MisterGames.Tick.Core;
using UnityEngine;

namespace MisterGames.Tick.Jobs.Structs {

    public static partial class Jobs {

        public static readonly Job Completed = default;

        private static IJobSystemProviders _jobSystemProviders;

        internal static void InjectJobSystemProviders(IJobSystemProviders providers) {
            _jobSystemProviders = providers;
        }

        internal static S GetJobSystem<S, T>(ITimeSource timeSource) where S : class, IJobSystem<T> {
            var jobSystem = _jobSystemProviders.GetProvider(timeSource).GetJobSystem<S, T>();
            if (jobSystem == null) Debug.LogError($"Job system of type {typeof(S)} is not found");
            return jobSystem;
        }

        internal static S GetJobSystem<S>(ITimeSource timeSource) where S : class, IJobSystem {
            var jobSystem = _jobSystemProviders.GetProvider(timeSource).GetJobSystem<S>();
            if (jobSystem == null) Debug.LogError($"Job system of type {typeof(S)} is not found");
            return jobSystem;
        }

        public static Job CreateJob<S, T>(ITimeSource timeSource, T data) where S : class, IJobSystem<T> {
            var jobSystem = GetJobSystem<S, T>(timeSource);
            if (jobSystem == null) return Completed;

            int jobId = jobSystem.CreateJob(data);
            return new Job(jobId, jobSystem);
        }

        public static Job CreateJob<S>(ITimeSource timeSource) where S : class, IJobSystem {
            var jobSystem = GetJobSystem<S>(timeSource);
            if (jobSystem == null) return Completed;

            int jobId = jobSystem.CreateJob();
            return new Job(jobId, jobSystem);
        }
    }

}
