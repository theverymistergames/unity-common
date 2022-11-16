namespace MisterGames.Tick.TimeProviders {

    public interface ITimeProviderFactory {
        ITimeProvider Create(TimerProviderType timerProviderType);
    }

}
