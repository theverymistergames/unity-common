using UnityEngine;

namespace MisterGames.Common.Tick {

    public interface IDeltaTimeProvider {
        float DeltaTime { get; }
    }

    public static class DeltaTimeProviders {

        public static readonly IDeltaTimeProvider Main = new MainUpdateProvider();
        public static readonly IDeltaTimeProvider Unscaled = new UnscaledUpdateProvider();
        public static readonly IDeltaTimeProvider Fixed = new FixedUpdateProvider();

        private sealed class MainUpdateProvider : IDeltaTimeProvider {
            public float DeltaTime => Time.deltaTime;
        }

        private sealed class UnscaledUpdateProvider : IDeltaTimeProvider {
            public float DeltaTime => Time.unscaledDeltaTime;
        }

        private sealed class FixedUpdateProvider : IDeltaTimeProvider {
            public float DeltaTime => Time.fixedDeltaTime;
        }
    }

}
