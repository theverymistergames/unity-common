using MisterGames.Tick.Core;
using UnityEngine;

namespace MisterGames.Tick.Jobs.Structs {

    public static partial class Jobs {

        public static readonly Job Completed = new Job(-1, null);

        internal static IJobSystemProviders JobSystemProviders;

        public static Job CreateJob<S, T>(ITimeSource timeSource, T data) where S : class, IJobSystem<T> {
            var jobSystem = JobSystemProviders.GetProvider(timeSource).GetJobSystem<S, T>();
            if (jobSystem == null) {
                Debug.LogError($"Job system of type {typeof(S)} is not found");
                return Completed;
            }

            int jobId = jobSystem.CreateJob(data);
            return new Job(jobId, jobSystem);
        }
    }

}
