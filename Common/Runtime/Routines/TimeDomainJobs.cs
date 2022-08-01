using System;

namespace MisterGames.Common.Routines {

    public static class TimeDomainJobs {

        public static IJob WaitFrame(this TimeDomain timeDomain) {
            return new EachFrameWhileJob(timeDomain, dt => false);
        }

        public static IJob WaitFrames(this TimeDomain timeDomain, int frames) {
            if (frames <= 0) return Jobs.Instant();

            int frameCounter = 0;
            return new EachFrameWhileJob(timeDomain, dt => ++frameCounter < frames);
        }

        public static IJob EachFrame(this TimeDomain timeDomain, Action action) {
            return Jobs.Do(action).Then(new EachFrameWhileJob(timeDomain, dt => {
                action.Invoke();
                return true;
            }));
        }

        public static IJob EachFrameWhile(this TimeDomain timeDomain, Func<bool> actionWhile) {
            return actionWhile.Invoke()
                ? new EachFrameWhileJob(timeDomain, dt => actionWhile.Invoke())
                : Jobs.Instant();
        }

        public static IJob Delay(this TimeDomain timeDomain, float seconds) {
            return new DelayJob(timeDomain, seconds);
        }

        public static IJob Schedule(this TimeDomain timeDomain, float startDelaySec, float periodSec, Action action) {
            return Jobs.Do(new DelayJob(timeDomain, startDelaySec))
                .Then(new ScheduleWhileJob(timeDomain, periodSec, () => {
                    action.Invoke();
                    return true;
                }));
        }

        public static IJob ScheduleWhile(this TimeDomain timeDomain, float startDelaySec, float periodSec, Func<bool> actionWhile) {
            return Jobs.Do(new DelayJob(timeDomain, startDelaySec))
                .Then(new ScheduleWhileJob(timeDomain, periodSec, actionWhile.Invoke));
        }

        public static IJob ScheduleTimes(this TimeDomain timeDomain, float startDelaySec, float periodSec, int repeatTimes, Action action) {
            int repeatCounter = 0;
            return Jobs.Do(new DelayJob(timeDomain, startDelaySec))
                .Then(new ScheduleWhileJob(timeDomain, periodSec, () => {
                    action.Invoke();
                    return ++repeatCounter < repeatTimes;
                }));
        }

        public static IJob ScheduleTimesWhile(this TimeDomain timeDomain, float startDelaySec, float periodSec, int repeatTimes, Func<bool> actionWhile) {
            int repeatCounter = 0;
            return Jobs.Do(new DelayJob(timeDomain, startDelaySec))
                .Then(new ScheduleWhileJob(timeDomain, periodSec, () => actionWhile.Invoke() && ++repeatCounter < repeatTimes));
        }

        public static IJob Process(this TimeDomain timeDomain, Func<float> getProcess, Action<float> action) {
            return new ProcessJob(timeDomain, getProcess, action);
        }

    }
}
