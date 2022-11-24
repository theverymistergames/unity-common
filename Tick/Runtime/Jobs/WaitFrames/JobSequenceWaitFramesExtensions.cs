namespace MisterGames.Tick.Jobs {

    public static class JobSequenceWaitFramesExtensions {

        public static JobSequence WaitFrames(this JobSequence sequence, int frames) {
            return sequence.Add(Jobs.WaitFrames(frames));
        }

        public static JobSequence<R> WaitFrames<R>(this JobSequence<R> sequence, int frames) {
            return sequence.Add(Jobs.WaitFrames(frames));
        }
    }

}
