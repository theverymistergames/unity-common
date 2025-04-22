using System;
using System.Collections.Generic;
using MisterGames.Common.Data;
using UnityEngine;

namespace MisterGames.Scenario.Events {

    public sealed class EventSystem : IEventSystem {
        
        public static readonly IEventSystem Main = new EventSystem();
        private static readonly EventReference RootEvent = new();
        
        public Dictionary<EventReference, int> RaisedEvents { get; } = new();
        private readonly TreeMap<EventReference, object> _listenerTree = new();

        public bool IsRaised(EventReference e) {
            return RaisedEvents.TryGetValue(e, out int count) && count > 0;
        }

        public int GetCount(EventReference e) {
            return RaisedEvents.GetValueOrDefault(e, 0);
        }

        public void Raise(EventReference e, int add = 1) {
            RaisedEvents[e] = RaisedEvents.GetValueOrDefault(e, 0) + add;
            NotifyEventRaised(e);
        }
        
        public void SetCount(EventReference e, int count) {
            RaisedEvents[e] = count;
            NotifyEventRaised(e);
        }

        public void Raise<T>(EventReference e, T data, int add = 1) {
            RaisedEvents[e] = RaisedEvents.GetValueOrDefault(e, 0) + add;
            NotifyEventRaised(e, data);
        }

        public void SetCount<T>(EventReference e, T data, int count) {
            RaisedEvents[e] = count;
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

        public void Subscribe<T>(IEventListener<T> listener) {
            SubscribeListener(RootEvent, listener);
        }

        public void Unsubscribe<T>(IEventListener<T> listener) {
            UnsubscribeListener(RootEvent, listener);
        }

        public void Subscribe(EventReference e, Action listener) {
            SubscribeListener(e, listener);
        }

        public void Unsubscribe(EventReference e, Action listener) {
            UnsubscribeListener(e, listener);
        }

        public void Subscribe<T>(EventReference e, Action<T> listener) {
            SubscribeListener(e, listener);
        }

        public void Unsubscribe<T>(EventReference e, Action<T> listener) {
            UnsubscribeListener(e, listener);
        }

        public void Subscribe<T>(Action<T> listener) {
            SubscribeListener(RootEvent, listener);
        }

        public void Unsubscribe<T>(Action<T> listener) {
            UnsubscribeListener(RootEvent, listener);
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
                switch (_listenerTree.GetValueAt(i)) {
                    case IEventListener interfaceListener:
                        interfaceListener.OnEventRaised(e);
                        break;

                    case Action actionListener:
                        actionListener.Invoke();
                        break;
                }
            }
        }
        
        private void NotifyEventRaised<T>(EventReference e, T data) {
            if (!_listenerTree.TryGetNode(e, out int root)) return;

            for (int i = _listenerTree.GetChild(root); i >= 0; i = _listenerTree.GetNext(i)) {
                switch (_listenerTree.GetValueAt(i)) {
                    case IEventListener<T> interfaceListener:
                        interfaceListener.OnEventRaised(e, data);
                        break;

                    case Action<T> actionListener:
                        actionListener.Invoke(data);
                        break;
                    
                    case IEventListener interfaceListener:
                        interfaceListener.OnEventRaised(e);
                        break;

                    case Action actionListener:
                        actionListener.Invoke();
                        break;
                }
            }
        }
    }
    
}
