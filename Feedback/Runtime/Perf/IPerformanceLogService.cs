namespace MisterGames.Feedback.Perf {

    public interface IPerformanceLogService {

        short CurrentFps { get; }
        short AverageFps { get; }
        short OnePercentFps { get; }
        short Zero1PercentFps { get; }

        /// <summary>
        /// Writes the current performance state into the console and into the feedback service at once,
        /// without waiting for the next scheduled log.
        /// </summary>
        void LogPerformance();
    }

}
