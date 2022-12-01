using System;

namespace MisterGames.Tick.Jobs.Structs {

    public static class JobSequenceExtensionsAction {

        public static JobSequence Action(this JobSequence jobSequence, Action action) {
            var job = Jobs.Action(jobSequence.timeSource, action);
            return jobSequence.Add(job);
        }

    }

}
