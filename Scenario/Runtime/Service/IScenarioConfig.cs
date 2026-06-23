using System.Collections.Generic;

namespace MisterGames.Scenario.Service {
    
    public interface IScenarioConfig {
    
        IReadOnlyList<ScenarioEvent> ScenarioEvents { get; }
        
    }
    
}