namespace MisterGames.Tick.Core {

    public interface ITimeSourceProvider {
        ITimeSource MainUpdate { get; }
        ITimeSource LateUpdate { get; }
        ITimeSource FixedUpdate { get; }
        ITimeSource UnscaledUpdate { get; }
    }

    public static class TimeSources {

        public static ITimeSource Main => _provider.MainUpdate;
        public static ITimeSource Late => _provider.LateUpdate;
        public static ITimeSource Fixed => _provider.FixedUpdate;
        public static ITimeSource Unscaled => _provider.UnscaledUpdate;

        private static ITimeSourceProvider _provider;

        internal static void InjectProvider(ITimeSourceProvider provider) {
            _provider = provider;
        }
    }

}
