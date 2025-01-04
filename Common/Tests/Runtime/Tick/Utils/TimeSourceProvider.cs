using System;
using MisterGames.Common.Tick;

namespace Utils {

    public class TimeSourceProvider : ITimeSourceProvider {
        private readonly Func<PlayerLoopStage, ITimeSource> _getTimeSource;

        public TimeSourceProvider(Func<PlayerLoopStage, ITimeSource> getTimeSource) {
            _getTimeSource = getTimeSource;
        }

        public ITimeSource Get(PlayerLoopStage stage) {
            return _getTimeSource.Invoke(stage);
        }

    }

}
