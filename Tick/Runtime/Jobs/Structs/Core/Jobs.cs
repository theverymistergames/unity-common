using MisterGames.Tick.Core;

namespace MisterGames.Tick.Jobs.Structs {

    public static partial class Jobs {

        public static readonly Job Completed = default;

        private static IJobSystemProviders _jobSystemProviders;

        internal static void InjectJobSystemProviders(IJobSystemProviders providers) {
            _jobSystemProviders = providers;
        }

        public static S GetJobSystem<S, T>(ITimeSource timeSource) where S : class, IJobSystem<T> {
            return _jobSystemProviders.GetProvider(timeSource).GetJobSystem<S, T>();
        }

        public static S GetJobSystem<S>(ITimeSource timeSource) where S : class, IJobSystem {
            return _jobSystemProviders.GetProvider(timeSource).GetJobSystem<S>();
        }

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
    }

}
