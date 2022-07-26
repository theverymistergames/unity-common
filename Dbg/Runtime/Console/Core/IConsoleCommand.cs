namespace MisterGames.Dbg.Console.Core {
    
    public interface IConsoleCommand {
        
        string Name { get; }
        string Description { get; }

        IConsoleCommandResult Process(DeveloperConsoleRunner runner, string[] args);
    }
}