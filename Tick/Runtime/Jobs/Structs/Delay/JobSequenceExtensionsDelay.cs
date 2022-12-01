﻿namespace MisterGames.Tick.Jobs.Structs {

    public static class JobSequenceExtensionsDelay {

        public static JobSequence Delay(this JobSequence jobSequence, float delay) {
            var job = Jobs.Delay(jobSequence.timeSource, delay);
            return jobSequence.Add(job);
        }

    }

}