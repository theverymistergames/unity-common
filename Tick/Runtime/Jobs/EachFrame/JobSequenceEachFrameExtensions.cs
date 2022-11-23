using System;

namespace MisterGames.Tick.Jobs {

    public static class JobSequenceEachFrameExtensions {

        public static JobSequence EachFrame(this JobSequence sequence, Action action) {
            return sequence.Add(Jobs.EachFrame(action));
        }

        public static JobSequence<R> EachFrame<R>(this JobSequence<R> sequence, Action action) {
            return sequence.Add(Jobs.EachFrame(action));
        }

        public static JobSequence EachFrameWhile(this JobSequence sequence, Func<bool> actionWhile) {
            return sequence.Add(Jobs.EachFrameWhile(actionWhile));
        }

        public static JobSequence<R> EachFrameWhile<R>(this JobSequence<R> sequence, Func<bool> actionWhile) {
            return sequence.Add(Jobs.EachFrameWhile(actionWhile));
        }
    }

}
