using UnityEngine;

namespace MisterGames.Tick.TimeProviders {

    internal readonly struct MainUpdate : ITimeProvider {
        public float UnscaledDeltaTime => Time.unscaledDeltaTime;
    }

}
