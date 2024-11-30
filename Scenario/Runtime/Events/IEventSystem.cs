using MisterGames.Common.Data;

namespace MisterGames.Scenario.Events {

    public interface IEventSystem {

        Map<EventReference, int> RaisedEvents { get; }

        void Raise(EventReference e, int add = 1);
        void SetCount(EventReference e, int count);
        
        void Raise<T>(EventReference e, T data, int add = 1);
        void SetCount<T>(EventReference e, T data, int count);

        void Subscribe(EventReference e, IEventListener listener);
        void Unsubscribe(EventReference e, IEventListener listener);
        
        void Subscribe<T>(EventReference e, IEventListener<T> listener);
        void Unsubscribe<T>(EventReference e, IEventListener<T> listener);
    }

}
