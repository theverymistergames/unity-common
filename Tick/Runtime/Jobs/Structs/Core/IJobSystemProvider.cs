using MisterGames.Tick.Core;

namespace MisterGames.Tick.Jobs.Structs {

    public interface IJobSystemProvider {
        S GetJobSystem<S, T>() where S : class, IJobSystem<T>;
        S GetJobSystem<S>() where S : class, IJobSystem;
    }

    public interface IJobSystemProviders {
        IJobSystemProvider GetProvider(ITimeSource timeSource);
    }
}
