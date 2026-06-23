namespace MisterGames.Scenario.Service {
    
    public interface IScenarioService {
        
        void AddScenario(IScenarioConfig scenarioConfig);
        
        void RemoveScenario(IScenarioConfig scenarioConfig);
    }
    
}