using System;
using MisterGames.Common.GameObjects;
using MisterGames.Common.Labels;
using MisterGames.Common.Service;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MisterGames.Common.State {
    
    public sealed class EnableStateSync : MonoBehaviour {
        
        [SerializeField] private LabelValue _group;
        [SerializeField] private Object[] _objects;
        [SerializeField] private Mode _mode;

        private enum Mode {
            Enable,
            Disable,
        }

        private void Awake() {
            Services.Get<IEnableStateService>()?.Subscribe(_group.GetValue(), Callback);
        }

        private void OnDestroy() {
            Services.Get<IEnableStateService>()?.Unsubscribe(_group.GetValue(), Callback);
        }

        private void Callback(bool state) {
            bool needEnable = _mode switch {
                Mode.Enable => state,
                Mode.Disable => !state,
                _ => throw new ArgumentOutOfRangeException()
            };
            
            _objects.SetEnabled(needEnable);
        }

#if UNITY_EDITOR
        private void Reset() {
            _objects = new Object[] { gameObject };
        }
#endif
    }
    
}