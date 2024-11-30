using System;
using System.Collections.Generic;
using MisterGames.Common.Data;
using UnityEngine;

namespace MisterGames.Scenario.Events {

    [Serializable]
    public sealed class EventSystem : IEventSystem {

        [SerializeField] private Map<EventReference, int> _raisedEvents = new();
        
        public Map<EventReference, int> RaisedEvents => _raisedEvents;
        private readonly TreeMap<EventReference, object> _listenerTree = new();
        
        public void Raise(EventReference e, int add = 1) {
            _raisedEvents[e] = _raisedEvents.GetValueOrDefault(e, 0) + add;
            NotifyEventRaised(e);
        }
        
        public void SetCount(EventReference e, int count) {
            _raisedEvents[e] = count;
            NotifyEventRaised(e);
        }

        public void Raise<T>(EventReference e, T data, int add = 1) {
            _raisedEvents[e] = _raisedEvents.GetValueOrDefault(e, 0) + add;
            NotifyEventRaised(e, data);
        }

        public void SetCount<T>(EventReference e, T data, int count) {
            _raisedEvents[e] = count;
            NotifyEventRaised(e, data);
        }

        public void Subscribe(EventReference e, IEventListener listener) {
            SubscribeListener(e, listener);
        }

        public void Unsubscribe(EventReference e, IEventListener listener) {
            UnsubscribeListener(e, listener);
        }

        public void Subscribe<T>(EventReference e, IEventListener<T> listener) {
            SubscribeListener(e, listener);
        }

        public void Unsubscribe<T>(EventReference e, IEventListener<T> listener) {
            UnsubscribeListener(e, listener);
        }

        private void SubscribeListener(EventReference e, object listener) {
            int root = _listenerTree.GetOrAddNode(e);

            for (int i = _listenerTree.GetChild(root); i >= 0; i = _listenerTree.GetNext(i)) {
                if (_listenerTree.GetValueAt(i) == listener) return;
            }

            _listenerTree.AddEndPoint(root, listener);
        }
        
        private void UnsubscribeListener(EventReference e, object listener) {
            if (!_listenerTree.TryGetNode(e, out int root)) return;

            for (int i = _listenerTree.GetChild(root); i >= 0; i = _listenerTree.GetNext(i)) {
                if (_listenerTree.GetValueAt(i) != listener) continue;

                _listenerTree.RemoveNodeAt(i);
                return;
            }
        }
        
        private void NotifyEventRaised(EventReference e) {
            if (!_listenerTree.TryGetNode(e, out int root)) return;

            for (int i = _listenerTree.GetChild(root); i >= 0; i = _listenerTree.GetNext(i)) {
                if (_listenerTree.GetValueAt(i) is IEventListener listener) listener.OnEventRaised(e);
            }
        }
        
        private void NotifyEventRaised<T>(EventReference e, T data) {
            if (!_listenerTree.TryGetNode(e, out int root)) return;

            for (int i = _listenerTree.GetChild(root); i >= 0; i = _listenerTree.GetNext(i)) {
                if (_listenerTree.GetValueAt(i) is IEventListener<T> listener) listener.OnEventRaised(e, data);
            }
        }
    }
    
}
