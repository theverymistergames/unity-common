using System.Collections.Generic;
using UnityEngine;

namespace MisterGames.Character.Fsm {

    public sealed class CharacterMotionFsmPipeline : MonoBehaviour, ICharacterMotionFsmPipeline {

        [SerializeField] private MonoBehaviour _motionFsm;

        private readonly Dictionary<object, bool> _enableMap = new Dictionary<object, bool>();

        public void Register(object source) {
            if (_enableMap.ContainsKey(source)) return;

            _enableMap.Add(source, true);
            _motionFsm.enabled = IsEnabled();
        }

        public void Unregister(object source) {
            if (!_enableMap.ContainsKey(source)) return;

            _enableMap.Remove(source);
            _motionFsm.enabled = IsEnabled();
        }

        public void SetEnabled(object source, bool isEnabled) {
            if (!_enableMap.ContainsKey(source)) return;

            _enableMap[source] = isEnabled;
            _motionFsm.enabled = IsEnabled();
        }

        private bool IsEnabled() {
            foreach (bool isEnabled in _enableMap.Values) {
                if (!isEnabled) return false;
            }

            return true;
        }
    }

}
