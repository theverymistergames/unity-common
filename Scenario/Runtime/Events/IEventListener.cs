namespace MisterGames.Scenario.Events {

    public interface IEventListener {
        void OnEventRaised(EventReference e);
    }

    public interface IEventListener<in T> {
        void OnEventRaised(EventReference e, T data);
    }
    
}
