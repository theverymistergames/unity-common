using System;
using MisterGames.Tick.Core;

namespace MisterGames.Tick.Jobs {

    public static partial class Jobs {

        public static Job Schedule(Action action, float period, int times = -1, PlayerLoopStage stage = PlayerLoopStage.Update) {
            return JobSystems.Get<JobSystemSchedule>(stage).CreateJob(action, period, times);
        }

        public static JobSequence Schedule(this JobSequence jobSequence, Action action, float period, int times = -1) {
            var job = Schedule(action, period, times, jobSequence.PlayerLoopStage);
            return jobSequence.Add(job);
        }
    }

}
