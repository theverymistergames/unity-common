namespace MisterGames.Common.Events {

    public interface IEventSystem {

        void Raise(object sender, Event e);

        void Raise<T>(object sender, Event e, T data);

        void Subscribe(Event e, IEventListener listener);

        void Subscribe<T>(Event e, IEventListener<T> listener);

        void Unsubscribe(Event e, IEventListener listener);

        void Unsubscribe<T>(Event e, IEventListener<T> listener);
    }

}
