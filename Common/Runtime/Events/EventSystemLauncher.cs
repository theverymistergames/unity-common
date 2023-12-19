using UnityEngine;

namespace MisterGames.Common.Events {

    public class EventSystemLauncher : MonoBehaviour {

        private EventSystem _eventSystem;

        private void Awake() {
            _eventSystem = new EventSystem();
            EventSystems.Global = _eventSystem;
        }
    }

}
