using System.Collections.Generic;
using MisterGames.Common.Data;

namespace MisterGames.Scenario.Events {

    public sealed class EventSystem : IEventSystem {

        public Dictionary<EventReference, int> RaisedEvents => _raisedEvents;

        private readonly Dictionary<EventReference, int> _raisedEvents;
        private readonly TreeMap<EventReference, IEventListener> _listenerTree;

        public EventSystem() {
            _listenerTree = new TreeMap<EventReference, IEventListener>();
            _raisedEvents = new Dictionary<EventReference, int>();
        }

        public void Raise(EventReference e) {
            AddEventRaiseCount(e, 1);
            NotifyEventRaised(e);
        }

        public void Subscribe(EventReference e, IEventListener listener) {
            int root = _listenerTree.GetOrAddNode(e);

            for (int i = _listenerTree.GetChild(root); i >= 0; i = _listenerTree.GetNext(i)) {
                if (_listenerTree.GetValueAt(i) == listener) return;
            }

            _listenerTree.AddEndPoint(root, listener);
        }

        public void Unsubscribe(EventReference e, IEventListener listener) {
            if (!_listenerTree.TryGetNode(e, out int root)) return;

            for (int i = _listenerTree.GetChild(root); i >= 0; i = _listenerTree.GetNext(i)) {
                if (_listenerTree.GetValueAt(i) != listener) continue;

                _listenerTree.RemoveNodeAt(i);
                return;
            }
        }

        private void AddEventRaiseCount(EventReference e, int add) {
            _raisedEvents[e] = (_raisedEvents.TryGetValue(e, out int count) ? count : 0) + add;
        }

        private void NotifyEventRaised(EventReference e) {
            if (!_listenerTree.TryGetNode(e, out int root)) return;

            for (int i = _listenerTree.GetChild(root); i >= 0; i = _listenerTree.GetNext(i)) {
                if (_listenerTree.GetValueAt(i) is { } listener) listener.OnEventRaised(e);
            }
        }
    }
    
}
