using System;
using MisterGames.Tick.Core;

namespace MisterGames.Tick.Jobs {

    public static partial class Jobs {

        public static Job EachFrame(Action<float> action, PlayerLoopStage stage = PlayerLoopStage.Update) {
            return JobSystems.Get<JobSystemEachFrame>(stage).CreateJob(action);
        }

        public static Job EachFrameWhile(Func<float, bool> actionWhile, PlayerLoopStage stage = PlayerLoopStage.Update) {
            return JobSystems.Get<JobSystemEachFrameWhile>(stage).CreateJob(actionWhile);
        }

        public static JobSequence EachFrame(this JobSequence jobSequence, Action<float> action, PlayerLoopStage stage = PlayerLoopStage.Update) {
            var job = EachFrame(action, stage);
            return jobSequence.Add(job);
        }

        public static JobSequence EachFrameWhile(this JobSequence jobSequence, Func<float, bool> actionWhile, PlayerLoopStage stage = PlayerLoopStage.Update) {
            var job = EachFrameWhile(actionWhile, stage);
            return jobSequence.Add(job);
        }
    }

}
