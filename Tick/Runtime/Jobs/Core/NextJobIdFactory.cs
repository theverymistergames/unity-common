namespace MisterGames.Tick.Jobs {

    public sealed class NextJobIdFactory : IJobIdFactory {

        private int _currentId;

        public int CreateNewJobId() {
            return _currentId++;
        }
    }

}
