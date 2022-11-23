using System;

namespace MisterGames.Tick.Jobs {

    public static class JobSequenceScheduleExtensions {

        public static JobSequence Schedule(this JobSequence sequence, float periodSec, Action action) {
            return sequence.Add(Jobs.Schedule(periodSec, action));
        }

        public static JobSequence<R> Schedule<R>(this JobSequence<R> sequence, float periodSec, Action action) {
            return sequence.Add(Jobs.Schedule(periodSec, action));
        }

        public static JobSequence ScheduleWhile(this JobSequence sequence, float periodSec, Func<bool> actionWhile) {
            return sequence.Add(Jobs.ScheduleWhile(periodSec, actionWhile));
        }

        public static JobSequence<R> ScheduleWhile<R>(this JobSequence<R> sequence, float periodSec, Func<bool> actionWhile) {
            return sequence.Add(Jobs.ScheduleWhile(periodSec, actionWhile));
        }

        public static JobSequence ScheduleTimes(this JobSequence sequence, float periodSec, int times, Action action) {
            return sequence.Add(Jobs.ScheduleTimes(periodSec, times, action));
        }

        public static JobSequence<R> ScheduleTimes<R>(this JobSequence<R> sequence, float periodSec, int times, Action action) {
            return sequence.Add(Jobs.ScheduleTimes(periodSec, times, action));
        }

        public static JobSequence ScheduleTimesWhile(this JobSequence sequence, float periodSec, int times, Func<bool> actionWhile) {
            return sequence.Add(Jobs.ScheduleTimesWhile(periodSec, times, actionWhile));
        }

        public static JobSequence<R> ScheduleTimesWhile<R>(this JobSequence<R> sequence, float periodSec, int times, Func<bool> actionWhile) {
            return sequence.Add(Jobs.ScheduleTimesWhile(periodSec, times, actionWhile));
        }
    }

}
