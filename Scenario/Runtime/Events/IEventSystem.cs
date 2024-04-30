using MisterGames.Common.Data;

namespace MisterGames.Scenario.Events {

    public interface IEventSystem {

        Map<EventReference, int> RaisedEvents { get; }

        void Raise(EventReference e, int add = 1);
        
        void SetCount(EventReference e, int count);

        void Subscribe(EventReference e, IEventListener listener);

        void Unsubscribe(EventReference e, IEventListener listener);
    }

}
