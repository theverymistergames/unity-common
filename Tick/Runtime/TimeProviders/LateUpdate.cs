using System;
using UnityEngine;

namespace MisterGames.Tick.TimeProviders {

    [Serializable]
    public readonly struct LateUpdate : ITimeProvider {
        float ITimeProvider.UnscaledDeltaTime => Time.unscaledDeltaTime;
    }

}
