namespace MisterGames.Tick.Jobs {

    public static partial class Jobs {

        public static IJob Delay(float seconds) {
            return new DelayJob(seconds);
        }
    }

}
