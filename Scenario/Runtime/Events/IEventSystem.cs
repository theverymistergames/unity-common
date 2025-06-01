using System;
using System.Collections.Generic;

namespace MisterGames.Scenario.Events {

    public interface IEventSystem : IDisposable {

        Dictionary<EventReference, int> RaisedEvents { get; }
        
        bool IsRaised(EventReference e);
        int GetCount(EventReference e);
        
        void Raise(EventReference e, int add = 1);
        void SetCount(EventReference e, int count);
        
        void Raise<T>(EventReference e, T data, int add = 1);
        void SetCount<T>(EventReference e, T data, int count);

        void ResetEventsOf(EventDomain eventDomain, bool includeSaved, bool notify);
        void ResetAllEvents(bool notify);
        
        void Subscribe(EventReference e, IEventListener listener);
        void Unsubscribe(EventReference e, IEventListener listener);
        
        void Subscribe<T>(EventReference e, IEventListener<T> listener);
        void Unsubscribe<T>(EventReference e, IEventListener<T> listener);
        
        void Subscribe<T>(IEventListener<T> listener);
        void Unsubscribe<T>(IEventListener<T> listener);
        
        void Subscribe(EventReference e, Action listener);
        void Unsubscribe(EventReference e, Action listener);
        
        void Subscribe<T>(EventReference e, Action<T> listener);
        void Unsubscribe<T>(EventReference e, Action<T> listener);
        
        void Subscribe<T>(Action<T> listener);
        void Unsubscribe<T>(Action<T> listener);
    }

}
