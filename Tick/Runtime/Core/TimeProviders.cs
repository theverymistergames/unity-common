using UnityEngine;

namespace MisterGames.Tick.Core {

    public interface ITimeProvider {
        float DeltaTime { get; }
        float TimeScale { get; set; }
    }

    internal static class TimeProviders {

        public static readonly ITimeProvider Main = new MainUpdateProvider();
        public static readonly ITimeProvider Unscaled = new UnscaledUpdateProvider();
        public static readonly ITimeProvider Fixed = new FixedUpdateProvider();

        private sealed class MainUpdateProvider : ITimeProvider {
            public float DeltaTime => Time.deltaTime;
            public float TimeScale { get => Time.timeScale; set => Time.timeScale = value; }
        }

        private sealed class UnscaledUpdateProvider : ITimeProvider {
            public float DeltaTime => Time.unscaledDeltaTime * TimeScale;
            public float TimeScale { get; set; } = 1f;
        }

        private sealed class FixedUpdateProvider : ITimeProvider {
            public float DeltaTime => Time.fixedUnscaledDeltaTime * TimeScale;
            public float TimeScale { get; set; } = 1f;
        }
    }
}
