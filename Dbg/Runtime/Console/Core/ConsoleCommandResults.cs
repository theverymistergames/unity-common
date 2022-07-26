using System;

namespace MisterGames.Dbg.Console.Core {

    public interface IConsoleCommandResult {
        
        string Output { get; }
        bool IsCompleted { get; }
        
    }
    
    public static class ConsoleCommandResults {
        
        public static readonly IConsoleCommandResult Empty = 
            Instant("");
        
        public static IConsoleCommandResult Instant(string output, bool isCompleted = true) => 
            new InstantResult(output, isCompleted);
        
        public static IConsoleCommandResult Continuous(Func<IConsoleCommandResult> getResult) => 
            new ContinuousResult(getResult);
     
        
        private class ContinuousResult : IConsoleCommandResult {
            public string Output => _getResult.Invoke().Output;
            public bool IsCompleted => _getResult.Invoke().IsCompleted;

            private readonly Func<IConsoleCommandResult> _getResult;

            public ContinuousResult(Func<IConsoleCommandResult> getResult) {
                _getResult = getResult;
            }
        }
    
        private readonly struct InstantResult : IConsoleCommandResult {
            public string Output { get; }
            public bool IsCompleted { get; }

            public InstantResult(string output, bool isCompleted) {
                Output = output;
                IsCompleted = isCompleted;
            }
        }
    }

}