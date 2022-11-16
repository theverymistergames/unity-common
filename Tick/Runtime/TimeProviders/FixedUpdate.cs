using System;
using UnityEngine;

namespace MisterGames.Tick.TimeProviders {

    [Serializable]
    public readonly struct FixedUpdate : ITimeProvider {
        float ITimeProvider.UnscaledDeltaTime => Time.fixedUnscaledDeltaTime;
    }

}
