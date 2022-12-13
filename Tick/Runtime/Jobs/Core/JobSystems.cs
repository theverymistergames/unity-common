using MisterGames.Tick.Core;

namespace MisterGames.Tick.Jobs {

    public interface IJobSystemProvider {
        S GetJobSystem<S>() where S : class, IJobSystemReadOnly;
    }

    public interface IJobSystemProviders {
        IJobSystemProvider GetStagedProvider(PlayerLoopStage stage);
        IJobSystemProvider GetProvider();
    }

    public static class JobSystems {

        private static IJobSystemProviders _jobSystemProviders;

        public static void InjectProvider(IJobSystemProviders providers) {
            _jobSystemProviders = providers;
        }

        public static S Get<S>(PlayerLoopStage stage) where S : class, IJobSystemReadOnly {
            return _jobSystemProviders.GetStagedProvider(stage).GetJobSystem<S>();
        }

        public static S Get<S>() where S : class, IJobSystemReadOnly {
            return _jobSystemProviders.GetProvider().GetJobSystem<S>();
        }
    }

}
