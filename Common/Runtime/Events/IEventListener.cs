namespace MisterGames.Common.Events {

    public interface IEventListener {
        void OnEventRaised(object sender, Event e);
    }

    public interface IEventListener<in T> {
        void OnEventRaised(object sender, Event e, T data);
    }

}
