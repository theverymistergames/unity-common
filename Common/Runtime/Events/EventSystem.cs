using MisterGames.Common.Data;

namespace MisterGames.Common.Events {

    public sealed class EventSystem : IEventSystem {

        private readonly TreeSet<object> _listenerTree;

        public EventSystem() {
            _listenerTree = new TreeSet<object>();
        }

        public void Raise(object sender, Event e) {
            if (!_listenerTree.TryGetNode(e, out int root)) return;

            for (int i = _listenerTree.GetChild(root); i >= 0; i = _listenerTree.GetNext(i)) {
                if (_listenerTree.GetKeyAt(i) is IEventListener listener) listener.OnEventRaised(sender, e);
            }
        }

        public void Raise<T>(object sender, Event e, T data) {
            if (!_listenerTree.TryGetNode(e, out int root)) return;

            for (int i = _listenerTree.GetChild(root); i >= 0; i = _listenerTree.GetNext(i)) {
                if (_listenerTree.GetKeyAt(i) is IEventListener<T> listener) listener.OnEventRaised(sender, e, data);
            }
        }

        public void Subscribe(Event e, IEventListener listener) {
            _listenerTree.GetOrAddNode(listener, parent: _listenerTree.GetOrAddNode(e));
        }

        public void Subscribe<T>(Event e, IEventListener<T> listener) {
            _listenerTree.GetOrAddNode(listener, parent: _listenerTree.GetOrAddNode(e));
        }

        public void Unsubscribe(Event e, IEventListener listener) {
            if (!_listenerTree.TryGetNode(e, out int root)) return;
            _listenerTree.RemoveNode(listener, root);
        }

        public void Unsubscribe<T>(Event e, IEventListener<T> listener) {
            if (!_listenerTree.TryGetNode(e, out int root)) return;
            _listenerTree.RemoveNode(listener, root);
        }
    }
    
}
