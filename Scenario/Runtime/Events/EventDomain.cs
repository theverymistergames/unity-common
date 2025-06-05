using System;
using System.Collections.Generic;
using UnityEngine;

namespace MisterGames.Scenario.Events {

    [CreateAssetMenu(fileName = nameof(EventDomain), menuName = "MisterGames/Events/" + nameof(EventDomain))]
    public sealed class EventDomain : ScriptableObject {

        [SerializeField] private EventGroup[] _eventGroups;
        [HideInInspector] [SerializeField] private int _nextId;
        
        internal EventGroup[] EventGroups => _eventGroups ?? Array.Empty<EventGroup>();
        
        private readonly Dictionary<int, (int, int)> _indexMap = new();
        private readonly Dictionary<int, (int, int)> _nameHashMap = new();
        
        [Serializable]
        internal struct EventGroup {
            public string name;
            public EventEntry[] events;
        }

        [Serializable]
        internal struct EventEntry {
            public string name;
            public int id;
            public int subId;
            public bool save;
        }

        public string GetEventName(int eventId) {
            if (!TryGetAddress(eventId, out int group, out int index)) return null;

            ref var g = ref _eventGroups[group];
            ref var e = ref g.events[index];

            return e.name;
        }

        public EventReference GetEventByName(string eventName) {
            if (!TryGetAddress(eventName, out int group, out int index)) return default;

            ref var g = ref _eventGroups[group];
            ref var e = ref g.events[index];

            return new EventReference(this, e.id);
        }

        internal bool IsSerializable(int eventId) {
            if (!TryGetAddress(eventId, out int group, out int index)) return false;

            ref var g = ref _eventGroups[group];
            ref var e = ref g.events[index];

            return e.save;
        }

        internal bool TryGetAddress(int eventId, out int group, out int index) {
#if UNITY_EDITOR
            if (_isEventPathMapInvalid) {
                _isEventPathMapInvalid = false;
                _indexMap.Clear();
            }
#endif

            if (_indexMap.TryGetValue(eventId, out (int group, int index) address)) {
                group = address.group;
                index = address.index;
                return true;
            }

            for (int i = 0; i < _eventGroups.Length; i++) {
                ref var g = ref _eventGroups[i];
                var events = g.events;

                for (int j = 0; j < events.Length; j++) {
                    ref var entry = ref events[j];
                    if (entry.id != eventId) continue;

                    group = i;
                    index = j;
                    _indexMap[eventId] = (i, j);
                    return true;
                }
            }

            group = 0;
            index = 0;
            return false;
        }
        
        private bool TryGetAddress(string eventName, out int group, out int index) {
            if (eventName == null) {
                group = 0;
                index = 0;
                return false;
            }
            
#if UNITY_EDITOR
            if (_isEventPathMapInvalid) {
                _isEventPathMapInvalid = false;
                _nameHashMap.Clear();
            }
#endif

            int hash = Animator.StringToHash(eventName);
            
            if (_nameHashMap.TryGetValue(hash, out (int group, int index) address)) {
                group = address.group;
                index = address.index;
                return true;
            }

            for (int i = 0; i < _eventGroups.Length; i++) {
                ref var g = ref _eventGroups[i];
                var events = g.events;

                for (int j = 0; j < events.Length; j++) {
                    ref var entry = ref events[j];
                    if (Animator.StringToHash(entry.name) != hash) continue;

                    group = i;
                    index = j;
                    _nameHashMap[hash] = (i, j);
                    return true;
                }
            }

            group = 0;
            index = 0;
            return false;
        }

#if UNITY_EDITOR
        private readonly HashSet<int> _occupiedIdsCache = new();
        private bool _isEventPathMapInvalid;
        
        private void OnValidate() {
            _isEventPathMapInvalid = true;
            _occupiedIdsCache.Clear();

            int id = _nextId;

            for (int i = 0; i < _eventGroups.Length; i++) {
                var events = _eventGroups[i].events;

                for (int j = 0; j < events.Length; j++) {
                    ref var entry = ref events[j];

                    if (entry.id == 0 || _occupiedIdsCache.Contains(entry.id)) {
                        if (id == 0) id++;
                        entry.id = id++;
                    }

                    _occupiedIdsCache.Add(entry.id);
                }
            }

            _nextId = id;
        }
#endif
    }

}
