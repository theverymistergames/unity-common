namespace MisterGames.Tick.Jobs.Structs {

    public static partial class Jobs {

        public static JobLaunchData<float, JobSystemDelay> Delay(float delay) {
            return new JobLaunchData<float, JobSystemDelay>(delay);
        }

    }

}
