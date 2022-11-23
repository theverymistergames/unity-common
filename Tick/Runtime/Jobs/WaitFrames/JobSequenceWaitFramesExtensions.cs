namespace MisterGames.Tick.Jobs {

    public static class JobSequenceWaitFramesExtensions {

        public static JobSequence WaitFrame(this JobSequence sequence) {
            return sequence.Add(Jobs.WaitFrames(1));
        }

        public static JobSequence<R> WaitFrame<R>(this JobSequence<R> sequence) {
            return sequence.Add(Jobs.WaitFrames(1));
        }

        public static JobSequence WaitFrames(this JobSequence sequence, int frames) {
            return sequence.Add(Jobs.WaitFrames(frames));
        }

        public static JobSequence<R> WaitFrames<R>(this JobSequence<R> sequence, int frames) {
            return sequence.Add(Jobs.WaitFrames(frames));
        }
    }

}
