using UnityEngine;

namespace MisterGames.Tick.TimeProviders {

    internal readonly struct FixedUpdate : ITimeProvider {
        public float UnscaledDeltaTime => Time.fixedUnscaledDeltaTime;
    }

}
