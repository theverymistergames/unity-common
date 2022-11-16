using System;

namespace MisterGames.Tick.TimeProviders {

    public class DefaultTimeProviderFactory : ITimeProviderFactory {

        public ITimeProvider Create(TimerProviderType timerProviderType) {
            return timerProviderType switch {
                TimerProviderType.MainUpdate => new MainUpdate(),
                TimerProviderType.LateUpdate => new LateUpdate(),
                TimerProviderType.FixedUpdate => new FixedUpdate(),
                _ => throw new NotImplementedException($"Cannot create time provider of type {timerProviderType}")
            };
        }

    }
}
