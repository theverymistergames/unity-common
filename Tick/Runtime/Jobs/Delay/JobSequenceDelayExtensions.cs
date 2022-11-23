namespace MisterGames.Tick.Jobs {

    public static class JobSequenceDelayExtensions {

        public static JobSequence Delay(this JobSequence sequence, float seconds) {
            return sequence.Add(Jobs.Delay(seconds));
        }

        public static JobSequence<R> Delay<R>(this JobSequence<R> sequence, float seconds) {
            return sequence.Add(Jobs.Delay(seconds));
        }
    }

}
