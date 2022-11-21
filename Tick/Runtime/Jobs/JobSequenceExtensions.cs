using System;

namespace MisterGames.Tick.Jobs {

    public static class JobSequenceExtensions {
        
        public static JobSequenceBuilder Action(this JobSequenceBuilder builder, Action action) {
            return builder.Add(Jobs.Action(action));
        }

        public static JobSequenceBuilder Delay(this JobSequenceBuilder builder, float seconds) {
            return builder.Add(Jobs.Delay(seconds));
        }

        public static JobSequenceBuilder WaitFrames(this JobSequenceBuilder builder, int frames) {
            return builder.Add(Jobs.WaitFrames(frames));
        }

        public static JobSequenceBuilder EachFrame(this JobSequenceBuilder builder, Action action) {
            return builder.Add(Jobs.EachFrame(action));
        }

        public static JobSequenceBuilder EachFrameWhile(this JobSequenceBuilder builder, Func<bool> actionWhile) {
            return builder.Add(Jobs.EachFrameWhile(actionWhile));
        }

        public static JobSequenceBuilder Schedule(this JobSequenceBuilder builder, float periodSec, Action action) {
            return builder.Add(Jobs.Schedule(periodSec, action));
        }

        public static JobSequenceBuilder ScheduleWhile(this JobSequenceBuilder builder, float periodSec, Func<bool> actionWhile) {
            return builder.Add(Jobs.ScheduleWhile(periodSec, actionWhile));
        }

        public static JobSequenceBuilder ScheduleTimes(this JobSequenceBuilder builder, float periodSec, int times, Action action) {
            return builder.Add(Jobs.ScheduleTimes(periodSec, times, action));
        }

        public static JobSequenceBuilder ScheduleTimesWhile(this JobSequenceBuilder builder, float periodSec, int times, Func<bool> actionWhile) {
            return builder.Add(Jobs.ScheduleTimesWhile(periodSec, times, actionWhile));
        }

        public static JobSequenceBuilder WaitCompletion(this JobSequenceBuilder builder, params IJobReadOnly[] jobs) {
            return builder.Add(Jobs.WaitCompletion(jobs));
        }

        public static JobSequenceBuilder Process(this JobSequenceBuilder builder, Func<float> getProcess, Action<float> action) {
            return builder.Add(Jobs.Process(getProcess, action));
        }
    }

}
