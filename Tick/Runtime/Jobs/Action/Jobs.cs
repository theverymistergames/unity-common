using System;

namespace MisterGames.Tick.Jobs {

    public static partial class Jobs {

        public static Job Action(Action action) {
            return JobSystems.Get<JobSystemAction>().CreateJob(action);
        }

        public static JobSequence Action(this JobSequence jobSequence, Action action) {
            var job = Action(action);
            return jobSequence.Add(job);
        }
    }

}
