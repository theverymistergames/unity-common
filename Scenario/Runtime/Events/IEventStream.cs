namespace MisterGames.Scenario.Events {
    
    public interface IEventStream<T> {
        
        void OnReadStream(ref T data);
    }
    
}