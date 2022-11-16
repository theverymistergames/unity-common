using System;
using MisterGames.Tick.Jobs;

namespace MisterGames.Tick.Utils {

    public static class JobSequenceExtensions {
        
        public static JobSequence Action(this JobSequence jobSequence, Action action) {
            return jobSequence.Add(Jobs.Action(action));
        }

        public static JobSequence Delay(this JobSequence jobSequence, float seconds) {
            return jobSequence.Add(Jobs.Delay(seconds));
        }

        public static JobSequence WaitFrames(this JobSequence jobSequence, int frames) {
            return jobSequence.Add(Jobs.WaitFrames(frames));
        }

        public static JobSequence EachFrame(this JobSequence jobSequence, Action action) {
            return jobSequence.Add(Jobs.EachFrame(action));
        }

        public static JobSequence EachFrameWhile(this JobSequence jobSequence, Func<bool> actionWhile) {
            return jobSequence.Add(Jobs.EachFrameWhile(actionWhile));
        }

        public static JobSequence Schedule(this JobSequence jobSequence, float periodSec, Action action) {
            return jobSequence.Add(Jobs.Schedule(periodSec, action));
        }

        public static JobSequence ScheduleWhile(this JobSequence jobSequence, float periodSec, Func<bool> actionWhile) {
            return jobSequence.Add(Jobs.ScheduleWhile(periodSec, actionWhile));
        }

        public static JobSequence Process(this JobSequence jobSequence, Func<float> getProcess, Action<float> action) {
            return jobSequence.Add(Jobs.Process(getProcess, action));
        }
    }

}
