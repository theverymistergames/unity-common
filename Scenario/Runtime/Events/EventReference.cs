using System;
using UnityEngine;

namespace MisterGames.Scenario.Events {

    [Serializable]
    public struct EventReference : IEquatable<EventReference> {

        [SerializeField] private EventDomain _eventDomain;
        [SerializeField] private int _eventId;

        public EventDomain EventDomain => _eventDomain;
        public int EventId => _eventId;

        public EventReference(EventDomain eventDomain, int eventId) {
            _eventDomain = eventDomain;
            _eventId = eventId;
        }

        public bool Equals(EventReference other) {
            return Equals(_eventDomain, other._eventDomain) && _eventId == other._eventId;
        }

        public override bool Equals(object obj) {
            return obj is EventReference other && Equals(other);
        }

        public override int GetHashCode() {
            return HashCode.Combine(_eventDomain, _eventId);
        }

        public static bool operator ==(EventReference left, EventReference right) {
            return left.Equals(right);
        }

        public static bool operator !=(EventReference left, EventReference right) {
            return !left.Equals(right);
        }
    }

}
