namespace MisterGames.Tick.Core {

    public interface ITimeSourceProvider {
        ITimeSource Get(PlayerLoopStage stage);
    }

    public enum PlayerLoopStage {
        PreUpdate,
        Update,
        UnscaledUpdate,
        LateUpdate,
        FixedUpdate,
    }

    public static class TimeSources {

        public static ITimeSource Get(PlayerLoopStage stage) => _provider.Get(stage);

        private static ITimeSourceProvider _provider;

        internal static void InjectProvider(ITimeSourceProvider provider) {
            _provider = provider;
        }
    }

}
