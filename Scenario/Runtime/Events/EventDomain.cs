using System;
using System.Collections.Generic;
using UnityEngine;

namespace MisterGames.Scenario.Events {

    [CreateAssetMenu(fileName = nameof(EventDomain), menuName = "MisterGames/Events/" + nameof(EventDomain))]
    public sealed class EventDomain : ScriptableObject {

        [SerializeField] private EventGroup[] _eventGroups;
        [HideInInspector] [SerializeField] private int _nextId;
        
        private readonly Dictionary<int, string> _eventPathMap = new Dictionary<int, string>();
        private bool _isEventPathMapInvalid;
        
        [Serializable]
        internal struct EventGroup {
            public string name;
            public EventEntry[] events;
        }

        [Serializable]
        internal struct EventEntry {
            public string name;
            [HideInInspector] public int id;
        }

        public string GetEventPath(int id) {
            if (!_isEventPathMapInvalid && _eventPathMap.TryGetValue(id, out string path)) return path;

            _isEventPathMapInvalid = false;
            _eventPathMap.Clear();

            for (int i = 0; i < _eventGroups.Length; i++) {
                ref var group = ref _eventGroups[i];
                var events = group.events;

                for (int j = 0; j < events.Length; j++) {
                    ref var entry = ref events[j];
                    if (entry.id != id) continue;

                    path = string.IsNullOrWhiteSpace(entry.name)
                        ? null
                        : string.IsNullOrWhiteSpace(group.name)
                            ? $"{entry.name}"
                            : $"{group.name}/{entry.name}";

                    _eventPathMap[id] = path;
                    return path;
                }
            }

            return null;
        }
        
#if UNITY_EDITOR
        private readonly HashSet<int> _occupiedIdsCache = new HashSet<int>();
        internal EventGroup[] EventGroups => _eventGroups;

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
