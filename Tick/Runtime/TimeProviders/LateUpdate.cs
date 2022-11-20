using UnityEngine;

namespace MisterGames.Tick.TimeProviders {

    internal readonly struct LateUpdate : ITimeProvider {
        public float UnscaledDeltaTime => Time.unscaledDeltaTime;
    }
}
