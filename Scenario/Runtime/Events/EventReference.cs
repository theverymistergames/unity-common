using System;
using UnityEngine;

namespace MisterGames.Scenario.Events {

    [Serializable]
    public struct EventReference : IEquatable<EventReference> {

        [SerializeField] private EventDomain _eventDomain;
        [SerializeField] private int _eventId;
        [SerializeField] private int _subId;

        public EventDomain EventDomain => _eventDomain;
        public int EventId => _eventId;
        public int SubId => _subId;
        public string Path => _eventDomain == null ? string.Empty : _eventDomain.GetEventPath(_eventId);

        public EventReference(EventDomain eventDomain, int eventId, int subId = 0) {
            _eventDomain = eventDomain;
            _eventId = eventId;
            _subId = subId;
        }

        public bool Equals(EventReference other) {
            return Equals(_eventDomain, other._eventDomain) && _eventId == other._eventId && _subId == other._subId;
        }

        public override bool Equals(object obj) {
            return obj is EventReference other && Equals(other);
        }

        public override int GetHashCode() {
            return HashCode.Combine(_eventDomain, _eventId, _subId);
        }

        public static bool operator ==(EventReference left, EventReference right) {
            return left.Equals(right);
        }

        public static bool operator !=(EventReference left, EventReference right) {
            return !left.Equals(right);
        }

        public EventReference WithSubId(int subId) {
            return new EventReference(_eventDomain, _eventId, subId);
        }
        
        public override string ToString() {
            return $"Event({Path})";
        }
    }

}
