using System;
using MisterGames.Tick.Core;

namespace MisterGames.Tick.Jobs {

    public static partial class Jobs {

        public static Job EachFrame(Action<float> action, int maxFrames = -1, PlayerLoopStage stage = PlayerLoopStage.Update) {
            return JobSystems.Get<JobSystemEachFrame>(stage).CreateJob(action, maxFrames);
        }

        public static Job EachFrameWhile(Func<float, bool> actionWhile, int maxFrames = -1, PlayerLoopStage stage = PlayerLoopStage.Update) {
            return JobSystems.Get<JobSystemEachFrameWhile>(stage).CreateJob(actionWhile, maxFrames);
        }

        public static JobSequence EachFrame(this JobSequence jobSequence, Action<float> action, int maxFrames = -1, PlayerLoopStage stage = PlayerLoopStage.Update) {
            var job = EachFrame(action, maxFrames, stage);
            return jobSequence.Add(job);
        }

        public static JobSequence EachFrameWhile(this JobSequence jobSequence, Func<float, bool> actionWhile, int maxFrames = -1, PlayerLoopStage stage = PlayerLoopStage.Update) {
            var job = EachFrameWhile(actionWhile, maxFrames, stage);
            return jobSequence.Add(job);
        }
    }

}
