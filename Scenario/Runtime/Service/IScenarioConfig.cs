using System.Collections.Generic;

namespace MisterGames.Scenario.Service {
    
    public interface IScenarioConfig {

        void CollectAllScenarioEvents(List<ScenarioEvent> buffer);

    }
    
}