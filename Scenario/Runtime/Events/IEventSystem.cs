using System.Collections.Generic;

namespace MisterGames.Scenario.Events {

    public interface IEventSystem {

        Dictionary<EventReference, int> RaisedEvents { get; }

        void Raise(EventReference e);

        void Subscribe(EventReference e, IEventListener listener);

        void Unsubscribe(EventReference e, IEventListener listener);
    }

}
