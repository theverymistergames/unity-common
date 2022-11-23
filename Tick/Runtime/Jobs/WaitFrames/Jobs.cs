namespace MisterGames.Tick.Jobs {

    public static partial class Jobs {

        public static IJob WaitFrames(int frames) {
            return new WaitFramesJob(frames);
        }

    }

}
