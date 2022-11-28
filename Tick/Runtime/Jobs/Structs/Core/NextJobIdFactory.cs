namespace MisterGames.Tick.Jobs.Structs {

    public sealed class NextJobIdFactory : IJobIdFactory {

        private int _lastId;

        public int CreateNewJobId() {
            return _lastId++;
        }
    }

}
