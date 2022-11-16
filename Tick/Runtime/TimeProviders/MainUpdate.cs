using System;
using UnityEngine;

namespace MisterGames.Tick.TimeProviders {

    [Serializable]
    public readonly struct MainUpdate : ITimeProvider {
        float ITimeProvider.UnscaledDeltaTime => Time.unscaledDeltaTime;
    }

}
