using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using MisterGames.Common.Data;
using MisterGames.Common.Strings;
using UnityEngine;
using UnityEngine.Pool;

namespace MisterGames.Scenario.Events {

    public sealed class EventBus : IEventBus {

        private const bool EnableLogs = false;
        
        public static readonly IEventBus Main = new EventBus();
        private static readonly EventReference GlobalEvent = new(eventDomain: null, eventId: -1);

        public Dictionary<EventReference, int> RaisedEvents { get; } = new();
        
        private readonly HashSet<EventReference> _subIdEventSet = new();
        private readonly TreeMap<EventReference, object> _listenerTree = new();
        private readonly MultiValueDictionary<Type, object> _streamMap = new();

        public void Dispose() {
            _listenerTree.Clear();
            _subIdEventSet.Clear();
            RaisedEvents.Clear();
            _streamMap.Clear();
        }

        public bool IsRaisedAtLeastOnce(EventReference e) {
            return RaisedEvents.ContainsKey(e);
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
            NotifyEventRaised(GlobalEvent, data);
        }

        public void AddStream<T>(IEventStream<T> stream) {
            _streamMap.AddValue(typeof(T), stream);
            
            if (EnableLogs) Log($"added stream [{stream}] for data type {nameof(T)}");
        }
        
        public void RemoveStream<T>(IEventStream<T> stream) {
            _streamMap.RemoveValue(typeof(T), stream);
            
            if (EnableLogs) Log($"removed stream [{stream}] for data type {nameof(T)}");
        }

        public void RequestData<T>(T defaultValue = default) {
            NotifyEventRaised(GlobalEvent, GetStreamData(defaultValue));
        }
        
        private T GetStreamData<T>(T value) {
            var type = typeof(T);
            int count = _streamMap.GetCount(type);

            for (int i = 0; i < count; i++) {
                if (_streamMap.GetValueAt(type, i) is not IEventStream<T> stream) continue;
                
                stream.OnReadStream(ref value);
            }
            
            return value;
        }
        
        public void ResetEventsOf(EventDomain eventDomain, bool notify) {
            if (EnableLogs) Log($"reset events of [{eventDomain}] (notify {notify})");
            
            List<EventReference> buffer = null;
            
            foreach (var e in _subIdEventSet) {
                if (e.EventDomain != eventDomain || !RaisedEvents.Remove(e)) continue;

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
            
            var groups = eventDomain.EventGroups;
            for (int i = 0; i < groups.Length; i++) {
                ref var group = ref groups[i];
                
                for (int j = 0; j < group.events.Length; j++) {
                    ref var evt = ref group.events[j];
                    var e = new EventReference(eventDomain, evt.id);
                    
                    if (RaisedEvents.Remove(e) && notify) NotifyEventRaised(e);
                }
            }
        }

        public void ResetAllEvents(bool notify) {
            if (EnableLogs) Log($"reset all events (notify {notify})");
            
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
        
        private void SetEventCount(EventReference e, int count) {
            RaisedEvents[e] = count;
            if (e.SubId != 0) _subIdEventSet.Add(e);
            
            if (EnableLogs) Log($"set count [{count}] for [{e}]");
        }

        private void NotifyEventRaised(EventReference e) {
            if (EnableLogs) Log($"raised [{e}] (no data)");
            
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
            if (EnableLogs) Log($"raised [{e}] with data [{data}]");
            
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
        
        public void Subscribe<T>(IEventListener<T> listener, bool forceNotify = false) {
            SubscribeListener(GlobalEvent, listener);
            if (forceNotify) RequestData<T>();
        }

        public void Unsubscribe<T>(IEventListener<T> listener) {
            UnsubscribeListener(GlobalEvent, listener);
        }

        public void Subscribe<T>(Action<T> listener, bool forceNotify = false) {
            SubscribeListener(GlobalEvent, listener);
            if (forceNotify) RequestData<T>();
        }

        public void Unsubscribe<T>(Action<T> listener) {
            UnsubscribeListener(GlobalEvent, listener);
        }

        private void SubscribeListener(EventReference e, object listener) {
            int root = _listenerTree.GetOrAddNode(e);
            
            for (int i = _listenerTree.GetChild(root); i >= 0; i = _listenerTree.GetNext(i)) {
                if (AreEqualListeners(_listenerTree.GetValueAt(i), listener)) return;
            }

            _listenerTree.AddEndPoint(root, listener);
            
            if (EnableLogs) Log($"added listener [{listener}] for [{e}]");
        }

        private void UnsubscribeListener(EventReference e, object listener) {
            if (!_listenerTree.TryGetNode(e, out int root)) return;
            
            for (int i = _listenerTree.GetChild(root); i >= 0; i = _listenerTree.GetNext(i)) {
                if (!AreEqualListeners(_listenerTree.GetValueAt(i), listener)) continue;

                _listenerTree.RemoveNodeAt(i);
                
                if (EnableLogs) Log($"removed listener [{listener}] for [{e}]");
                return;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool AreEqualListeners(object l0, object l1) {
            return l0 is not null && l0.Equals(l1);
        }

        [HideInCallstack]
        private static void Log(string message) {
            Debug.Log($"{nameof(EventBus).FormatColorOnlyForEditor(Color.white)}: f {Time.frameCount}, {message}");
        }
    }
    
}
