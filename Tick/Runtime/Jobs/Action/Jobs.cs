using System;
using MisterGames.Tick.Core;

namespace MisterGames.Tick.Jobs {

    public static partial class Jobs {

        public static Job Action(Action action, PlayerLoopStage stage = PlayerLoopStage.Update) {
            return JobSystems.Get<JobSystemAction>(stage).CreateJob(action);
        }

        public static JobSequence Action(this JobSequence jobSequence, Action action, PlayerLoopStage stage = PlayerLoopStage.Update) {
            var job = Action(action, stage);
            return jobSequence.Add(job);
        }
    }

}
