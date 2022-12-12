using MisterGames.Tick.Core;

namespace MisterGames.Tick.Jobs {

    public static partial class Jobs {

        public static Job Delay(float delay, PlayerLoopStage stage = PlayerLoopStage.Update) {
            return JobSystems.Get<JobSystemDelay>(stage).CreateJob(delay);
        }

        public static JobSequence Delay(this JobSequence jobSequence, float delay, PlayerLoopStage stage = PlayerLoopStage.Update) {
            var job = Delay(delay, stage);
            return jobSequence.Add(job);
        }
    }

}
