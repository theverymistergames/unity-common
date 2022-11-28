using MisterGames.Tick.Core;

namespace MisterGames.Tick.Jobs.Structs {

    public readonly struct JobLaunchData<S> where S : class, IJobSystem {

        public Job StartFrom(ITimeSource timeSource) {
            var jobSystem = JobSystemProviders.Instance.GetProvider(timeSource).GetJobSystem<S>();
            int jobId = jobSystem.CreateJob();

            jobSystem.StartJob(jobId);

            return new Job(jobId, jobSystem);
        }
    }

    public readonly struct JobLaunchData<T, S> where S : class, IJobSystem<T> {

        private readonly T _data;

        public JobLaunchData(T data) {
            _data = data;
        }

        public Job StartFrom(ITimeSource timeSource) {
            var jobSystem = JobSystemProviders.Instance.GetProvider(timeSource).GetJobSystem<S, T>();
            int jobId = jobSystem.CreateJob(_data);

            jobSystem.StartJob(jobId);

            return new Job(jobId, jobSystem);
        }
    }

}
