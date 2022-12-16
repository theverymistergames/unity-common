using UnityEngine;

namespace MisterGames.Tick.Core {

    public interface ITimeScaleProvider {
        float TimeScale { get; set; }
    }

    public static class TimeScaleProviders {

        public static readonly ITimeScaleProvider Global = new GlobalTimeScaleProvider();
        public static ITimeScaleProvider Create() => new IndependentTimeScaleProvider();

        private sealed class GlobalTimeScaleProvider : ITimeScaleProvider {
            public float TimeScale { get => Time.timeScale; set => Time.timeScale = value; }
        }

        private sealed class IndependentTimeScaleProvider : ITimeScaleProvider {
            public float TimeScale { get; set; } = 1f;
        }
    }

}
