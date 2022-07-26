﻿namespace MisterGames.Tick.Core {

    public interface ITimeSourceProvider {
        ITimeSource Get(PlayerLoopStage stage);
    }

    public static class TimeSources {

        public static ITimeSource Get(PlayerLoopStage stage) => _provider.Get(stage);

        private static ITimeSourceProvider _provider;

        public static void InjectProvider(ITimeSourceProvider provider) {
            _provider = provider;
        }
    }

}
