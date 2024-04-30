using MisterGames.Common.Save;
using UnityEngine;

namespace MisterGames.Scenario.Events {
    
    public class EventSystemLauncher : MonoBehaviour, ISaveable {

        [SerializeField] private string _id;
        private EventSystem _eventSystem;

        private void Awake() {
            _eventSystem = new EventSystem();
            EventSystems.Global = _eventSystem;
        }

        private void OnEnable() {
            SaveSystem.Instance.Register(this);
        }

        private void OnDisable() {
            SaveSystem.Instance.Unregister(this);
        }

        public void OnLoadData(ISaveSystem saveSystem) {
            saveSystem.Pop(_id, _eventSystem, out _eventSystem);
            EventSystems.Global = _eventSystem;
        }

        public void OnSaveData(ISaveSystem saveSystem) {
            saveSystem.Push(_id, _eventSystem);
        }
    }

}
