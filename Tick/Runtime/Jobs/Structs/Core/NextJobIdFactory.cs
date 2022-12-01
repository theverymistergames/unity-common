namespace MisterGames.Tick.Jobs.Structs {

    public sealed class NextJobIdFactory : IJobIdFactory {

        private int _currentId;

        public int CreateNewJobId() {
            return _currentId++;
        }
    }

}
