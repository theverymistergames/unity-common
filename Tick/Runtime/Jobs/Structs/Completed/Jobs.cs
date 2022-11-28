namespace MisterGames.Tick.Jobs.Structs {

    public static partial class Jobs {
        
        public static readonly Job Completed = new Job(-1, new JobSystemCompleted());

    }

}
