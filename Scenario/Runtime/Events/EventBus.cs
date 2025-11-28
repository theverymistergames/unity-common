using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using MisterGames.Common.Data;
using UnityEngine.Pool;

namespace MisterGames.Scenario.Events {

    public sealed class EventBus : IEventBus {
        
        public static readonly IEventBus Main = new EventBus();
        private static readonly EventReference RootEvent = new();

        public Dictionary<EventReference, int> RaisedEvents { get; } = new();
        
        private readonly HashSet<EventReference> _subIdEventSet = new();
        private readonly TreeMap<EventReference, object> _listenerTree = new();

        public void Dispose() {
            _listenerTree.Clear();
            _subIdEventSet.Clear();
            RaisedEvents.Clear();
        }

        public bool IsRaised(EventReference e) {
            return RaisedEvents.TryGetValue(e, out int count) && count > 0;
        }

        public int GetCount(EventReference e) {
            return RaisedEvents.GetValueOrDefault(e, 0);
        }

        public void Raise(EventReference e, int add = 1) {
            SetEventCount(e, RaisedEvents.GetValueOrDefault(e, 0) + add);
            NotifyEventRaised(e);
        }
        
        public void SetCount(EventReference e, int count) {
            SetEventCount(e, count);
            NotifyEventRaised(e);
        }

        public void Raise<T>(EventReference e, T data, int add = 1) {
            SetEventCount(e, RaisedEvents.GetValueOrDefault(e, 0) + add);
            NotifyEventRaised(e, data);
        }

        public void SetCount<T>(EventReference e, T data, int count) {
            SetEventCount(e, count);
            NotifyEventRaised(e, data);
        }

        public void RaiseGlobal<T>(T data) {
            SetEventCount(RootEvent, RaisedEvents.GetValueOrDefault(RootEvent, 0) + 1);
            NotifyEventRaised(RootEvent, data);
        }

        public void ResetEventsOf(EventDomain eventDomain, bool includeSaved, bool notify) {
            var groups = eventDomain.EventGroups;
            List<EventReference> buffer = null;
            
            foreach (var e in _subIdEventSet) {
                if (e.EventDomain == null || !includeSaved && e.EventDomain.IsSerializable(e.EventId) || !RaisedEvents.Remove(e)) continue;

                buffer ??= ListPool<EventReference>.Get();
                buffer.Add(e);
                
                if (notify) NotifyEventRaised(e);
            }

            if (buffer != null) {
                for (int i = 0; i < buffer.Count; i++) {
                    _subIdEventSet.Remove(buffer[i]);
                }
                ListPool<EventReference>.Release(buffer);
            }
            
            for (int i = 0; i < groups.Length; i++) {
                ref var group = ref groups[i];
                
                for (int j = 0; j < group.events.Length; j++) {
                    ref var evt = ref group.events[j];
                    var e = new EventReference(eventDomain, evt.id);
                    
                    if ((includeSaved || !evt.save) && RaisedEvents.Remove(e) && notify) NotifyEventRaised(e);
                }
            }
        }

        public void ResetAllEvents(bool notify) {
            if (!notify) {
                RaisedEvents.Clear();
                _subIdEventSet.Clear();
                return;
            }

            int count = RaisedEvents.Count;
            int i = 0;
            
            var buffer = ArrayPool<EventReference>.Shared.Rent(count);
            foreach (var e in RaisedEvents.Keys) {
                buffer[i++] = e;
            }

            RaisedEvents.Clear();
            _subIdEventSet.Clear();
            
            for (i = 0; i < count; i++) {
                NotifyEventRaised(buffer[i]);
            }
            
            ArrayPool<EventReference>.Shared.Return(buffer);
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

        private void SetEventCount(EventReference e, int count) {
            if (count == 0) {
                RaisedEvents.Remove(e);
                if (e.SubId != 0) _subIdEventSet.Remove(e);
                return;
            }

            RaisedEvents[e] = count;
            if (e.SubId != 0) _subIdEventSet.Add(e);
        }
        
        private void SubscribeListener(EventReference e, object listener) {
            int root = _listenerTree.GetOrAddNode(e);
            
            for (int i = _listenerTree.GetChild(root); i >= 0; i = _listenerTree.GetNext(i)) {
                if (AreEqualListeners(_listenerTree.GetValueAt(i), listener)) return;
            }

            _listenerTree.AddEndPoint(root, listener);
        }
        
        private void UnsubscribeListener(EventReference e, object listener) {
            if (!_listenerTree.TryGetNode(e, out int root)) return;
            
            for (int i = _listenerTree.GetChild(root); i >= 0; i = _listenerTree.GetNext(i)) {
                if (!AreEqualListeners(_listenerTree.GetValueAt(i), listener)) continue;

                _listenerTree.RemoveNodeAt(i);
                return;
            }
        }
        
        private void NotifyEventRaised(EventReference e) {
            if (!_listenerTree.TryGetNode(e, out int root)) return;

            int i = _listenerTree.GetChild(root);
            while (i >= 0) {
                int next = _listenerTree.GetNext(i);
                
                switch (_listenerTree.GetValueAt(i)) {
                    case IEventListener interfaceListener:
                        interfaceListener.OnEventRaised(e);
                        break;

                    case Action actionListener:
                        actionListener.Invoke();
                        break;
                }

                i = next;
            }
        }
        
        private void NotifyEventRaised<T>(EventReference e, T data) {
            if (!_listenerTree.TryGetNode(e, out int root)) return;
            
            int i = _listenerTree.GetChild(root);
            while (i >= 0) {
                int next = _listenerTree.GetNext(i);
                
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

                i = next;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool AreEqualListeners(object l0, object l1) {
            return l0 is not null && l0.Equals(l1);
        }
    }
    
}
