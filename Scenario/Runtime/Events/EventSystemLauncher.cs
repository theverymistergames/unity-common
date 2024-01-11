using UnityEngine;

namespace MisterGames.Scenario.Events {

    public class EventSystemLauncher : MonoBehaviour {

        private EventSystem _eventSystem;

        private void Awake() {
            _eventSystem = new EventSystem();
            EventSystems.Global = _eventSystem;
        }
    }

}
