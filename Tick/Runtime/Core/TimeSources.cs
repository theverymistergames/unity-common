namespace MisterGames.Tick.Core {

    public interface ITimeSourceProvider {
        ITimeSource PreUpdate { get; }
        ITimeSource Update { get; }
        ITimeSource UnscaledUpdate { get; }
        ITimeSource LateUpdate { get; }
        ITimeSource FixedUpdate { get; }
    }

    public static class TimeSources {

        public static ITimeSource PreUpdate => _provider.PreUpdate;
        public static ITimeSource Update => _provider.Update;
        public static ITimeSource UnscaledUpdate => _provider.UnscaledUpdate;
        public static ITimeSource LateUpdate => _provider.LateUpdate;
        public static ITimeSource FixedUpdate => _provider.FixedUpdate;

        private static ITimeSourceProvider _provider;

        internal static void InjectProvider(ITimeSourceProvider provider) {
            _provider = provider;
        }
    }

}
