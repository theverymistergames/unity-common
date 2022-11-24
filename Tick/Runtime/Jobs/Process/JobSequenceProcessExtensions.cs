using System;

namespace MisterGames.Tick.Jobs {

    public static class JobSequenceProcessExtensions {

        public static JobSequence Process(this JobSequence sequence, Func<float> getProcess, Action<float> action) {
            return sequence.Add(Jobs.EachFrameProcess(getProcess, action));
        }

        public static JobSequence<R> Process<R>(this JobSequence<R> sequence, Func<float> getProcess, Action<float> action) {
            return sequence.Add(Jobs.EachFrameProcess(getProcess, action));
        }
    }

}
