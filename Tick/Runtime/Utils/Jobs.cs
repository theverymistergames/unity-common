using System;
using MisterGames.Tick.Jobs;

namespace MisterGames.Tick.Utils {

    public static class Jobs {

        public static readonly IJob Completed = new CompletedJob();

        public static IJob Action(Action action) {
            return new InstantJob(action);
        }

        public static IJob Delay(float seconds) {
            return new DelayJob(seconds);
        }

        public static IJob WaitFrames(int frames) {
            return new WaitFramesJob(frames);
        }

        public static IJob EachFrame(Action action) {
            return new EachFrameWhileJob(() => {
                action.Invoke();
                return true;
            });
        }

        public static IJob EachFrameWhile(Func<bool> actionWhile) {
            return new EachFrameWhileJob(actionWhile);
        }

        public static IJob Schedule(float periodSec, Action action) {
            return new ScheduleWhileJob(periodSec, () => {
                action.Invoke();
                return true;
            });
        }

        public static IJob ScheduleWhile(float periodSec, Func<bool> actionWhile) {
            return new ScheduleWhileJob(periodSec, actionWhile);
        }

        public static IJob ScheduleTimes(float periodSec, int times, Action action) {
            if (times < 1) return Completed;

            return new ScheduleWhileJob(periodSec, () => {
                action.Invoke();
                return --times > 0;
            });
        }

        public static IJob ScheduleTimesWhile(float periodSec, int times, Func<bool> actionWhile) {
            if (times < 1) return Completed;

            return new ScheduleWhileJob(periodSec, () => actionWhile.Invoke() && --times > 0);
        }

        public static IJob WaitCompletion(params IJobReadOnly[] jobs) {
            return new WaitCompletionJob(jobs);
        }

        public static IJob Process(Func<float> getProcess, Action<float> action) {
            return new ProcessJob(getProcess, action);
        }
    }

}
