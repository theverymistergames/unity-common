using MisterGames.Tick.Core;

namespace MisterGames.Tick.Jobs {

    public static partial class Jobs {

        public static readonly Job Completed = default;



/*
        public static Job CreateJob<S, T>(ITimeSource timeSource, T data) where S : class, IJobSystem<T> {
            var jobSystem = GetJobSystem<S, T>(timeSource);
            int jobId = jobSystem.CreateJob(data);
            return new Job(jobId, jobSystem);
        }

        public static Job CreateJob<S>(ITimeSource timeSource) where S : class, IJobSystem {
            var jobSystem = GetJobSystem<S>(timeSource);
            int jobId = jobSystem.CreateJob();
            return new Job(jobId, jobSystem);
        }
        */
    }

}
