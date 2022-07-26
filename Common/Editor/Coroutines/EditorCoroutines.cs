using System;
using System.Collections;
using Unity.EditorCoroutines.Editor;

namespace MisterGames.Common.Editor.Coroutines {

    public static class EditorCoroutines {

        public static EditorCoroutineTask StartCoroutine(object owner, IEnumerator routine) {
            var task = new EditorCoroutineTask(owner, routine);
            task.Start();
            return task;
        }

        public static IEnumerator NextFrame(Action action) {
            yield return null;
            action.Invoke();
        }

        public static IEnumerator EachFrameWhile(Func<bool> actionWhile, Action onFinish = null) {
            while (actionWhile.Invoke()) {
                yield return null;
            }
            onFinish?.Invoke();
        }

        public static IEnumerator Delay(float sec, Action action) {
            yield return new EditorWaitForSeconds(sec);
            action.Invoke();
        }

        public static IEnumerator ScheduleWhile(float startDelaySec, float periodSec, Func<bool> actionWhile, Action onFinish = null) {
            yield return new EditorWaitForSeconds(startDelaySec);
            var period = new EditorWaitForSeconds(periodSec);
            while (actionWhile.Invoke()) {
                yield return period;
            }
            onFinish?.Invoke();
        }
        
        public static IEnumerator ScheduleTimes(float startDelaySec, float periodSec, int repeatTimes, Action action, Action onFinish = null) {
            var repeatCounter = 0;
            var actionWhile = new Func<bool>(() => {
                if (repeatCounter++ >= repeatTimes) return false;
                action.Invoke();
                return true;
            });
            return ScheduleWhile(startDelaySec, periodSec, actionWhile, onFinish);
        }
        
        public static IEnumerator ScheduleTimesWhile(float startDelaySec, float periodSec, int repeatTimes, Func<bool> actionWhile, Action onFinish = null) {
            var repeatCounter = 0;
            var actionWhileRepeat = new Func<bool>(() => repeatCounter++ < repeatTimes && actionWhile.Invoke());
            return ScheduleWhile(startDelaySec, periodSec, actionWhileRepeat, onFinish);
        }

    }

}