using UnityEngine;

namespace MisterGames.Common.Events {

    [CreateAssetMenu(fileName = nameof(Event), menuName = "MisterGames/" + nameof(Event))]
    public class Event : ScriptableObject {

        public void Raise(object sender) {
            EventSystems.Global.Raise(sender, this);
        }

        public void Raise<T>(object sender, T data) {
            EventSystems.Global.Raise(sender, this, data);
        }

        public void Subscribe(IEventListener listener) {
            EventSystems.Global.Subscribe(this, listener);
        }

        public void Subscribe<T>(IEventListener<T> listener) {
            EventSystems.Global.Subscribe(this, listener);
        }

        public void Unsubscribe(IEventListener listener) {
            EventSystems.Global.Unsubscribe(this, listener);
        }

        public void Unsubscribe<T>(IEventListener<T> listener) {
            EventSystems.Global.Unsubscribe(this, listener);
        }
    }

}
