﻿using System;

namespace MisterGames.Tick.Jobs {

    public static partial class Jobs {

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
            return times < 1
                ? Completed
                : new ScheduleTimesWhileJob(periodSec, times, () => {
                    action.Invoke();
                    return true;
                });
        }

        public static IJob ScheduleTimesWhile(float periodSec, int times, Func<bool> actionWhile) {
            return times < 1
                ? Completed
                : new ScheduleTimesWhileJob(periodSec, times, actionWhile);
        }
    }

}
