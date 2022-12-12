using MisterGames.Tick.Core;

namespace MisterGames.Tick.Jobs {

    public interface IJobSystemProvider {
        S GetJobSystem<S>() where S : class, IJobSystemBase;
    }

    public interface IJobSystemProviders {
        IJobSystemProvider GetProvider(PlayerLoopStage stage);
    }

    public static class JobSystems {

        private static IJobSystemProviders _jobSystemProviders;

        internal static void InjectProvider(IJobSystemProviders providers) {
            _jobSystemProviders = providers;
        }

        public static S Get<S>(PlayerLoopStage stage) where S : class, IJobSystemBase {
            return _jobSystemProviders.GetProvider(stage).GetJobSystem<S>();
        }
    }

}
