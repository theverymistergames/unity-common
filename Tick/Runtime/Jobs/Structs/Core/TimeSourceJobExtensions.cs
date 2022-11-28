using MisterGames.Tick.Core;

namespace MisterGames.Tick.Jobs.Structs {

    public static class TimeSourceJobExtensions {

        public static Job StartJob<T, S>(this ITimeSource timeSource, JobLaunchData<T, S> data) where S : class, IJobSystem<T> {
            return data.StartFrom(timeSource);
        }

        public static Job StartJob<S>(this ITimeSource timeSource, JobLaunchData<S> data) where S : class, IJobSystem {
            return data.StartFrom(timeSource);
        }

    }
}
