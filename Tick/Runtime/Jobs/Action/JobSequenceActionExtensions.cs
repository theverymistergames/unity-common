using System;

namespace MisterGames.Tick.Jobs {

    public static class JobSequenceActionExtensions {

        public static JobSequence Action(this JobSequence sequence, Action action) {
            return sequence.Add(Jobs.Action(action));
        }

        public static JobSequence<R> Action<R>(this JobSequence sequence, Func<R> func) {
            return sequence.Add(Jobs.Action(func));
        }

        public static JobSequence<R> Action<R>(this JobSequence sequence, Func<R> func, out IJobReadOnly<R> resultJob) {
            return sequence.Add(Jobs.Action(func), out resultJob);
        }

        public static JobSequence<R> Action<R>(this JobSequence<R> sequence, Action action) {
            return sequence.Add(Jobs.Action(action));
        }

        public static JobSequence<R> Action<R>(this JobSequence<R> sequence, Func<R> func) {
            return sequence.Add(Jobs.Action(func));
        }

        public static JobSequence<R> Action<R>(this JobSequence<R> sequence, Func<R> func, out IJobReadOnly<R> resultJob) {
            return sequence.Add(Jobs.Action(func), out resultJob);
        }

        public static JobSequence<T> Action<T, R>(this JobSequence<R> sequence, Func<T> func) {
            return sequence.Add(Jobs.Action(func));
        }

        public static JobSequence<T> Action<T, R>(this JobSequence<R> sequence, Func<T> func, out IJobReadOnly<T> resultJob) {
            return sequence.Add(Jobs.Action(func), out resultJob);
        }
    }

}
