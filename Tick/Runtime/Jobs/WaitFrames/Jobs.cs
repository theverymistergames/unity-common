using MisterGames.Tick.Core;

namespace MisterGames.Tick.Jobs {

    public static partial class Jobs {

        public static Job WaitFrames(int frames, PlayerLoopStage stage = PlayerLoopStage.Update) {
            return JobSystems.Get<JobSystemWaitFrames>(stage).CreateJob(frames);
        }

        public static JobSequence WaitFrames(this JobSequence jobSequence, int frames, PlayerLoopStage stage = PlayerLoopStage.Update) {
            var job = WaitFrames(frames, stage);
            return jobSequence.Add(job);
        }
    }

}
