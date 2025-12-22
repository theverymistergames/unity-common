using System;
using MisterGames.Common.Attributes;
using MisterGames.Common.GameObjects;
using UnityEngine;
using Object = UnityEngine.Object;

#if UNITY_EDITOR
using Cysharp.Threading.Tasks;
using MisterGames.Common.Build;
using UnityEditor;
#endif

namespace MisterGames.Logic.GameObjects {
    
    [ExecuteInEditMode]
    public sealed class EnableBehaviour : MonoBehaviour {
        
        [SerializeField] private Mode _mode = Mode.SyncMonoBehaviourEnable;
        [VisibleIf(nameof(_mode), 1)]
        [SerializeField] private State _state = State.Enabled;
        [SerializeField] private EnableGroup[] _enableGroups;
        
        [Serializable]
        private struct EnableGroup {
            public State state;
            public Object[] objects;
        }

        private enum Mode {
            SyncMonoBehaviourEnable,
            Manual,
        }
        
        private enum State {
            Enabled,
            Disabled,
        }

        private void Awake() {
            if (_mode == Mode.Manual) SetStateInternal(enabled, notifyMonoBehaviour: true);
        }

        private void OnEnable() {
            if (_mode == Mode.SyncMonoBehaviourEnable) SetStateInternal(enabled: true, notifyMonoBehaviour: false);
        }

        private void OnDisable() {
            if (_mode == Mode.SyncMonoBehaviourEnable) SetStateInternal(enabled: false, notifyMonoBehaviour: false);
        }
        
        public void SetState(bool enabled) {
            SetStateInternal(enabled, notifyMonoBehaviour: true);
        }

        private void SetStateInternal(bool enabled, bool notifyMonoBehaviour) {
#if UNITY_EDITOR
            if (this == null) return;
            
            bool wasEnabled = this.enabled;
            var lastState = _state;
#endif
            
            _state = enabled ? State.Enabled : State.Disabled;
            
            if (notifyMonoBehaviour && _mode == Mode.SyncMonoBehaviourEnable) {
                this.enabled = enabled;
            }
            
#if UNITY_EDITOR
            if (!Application.isPlaying && (lastState != _state || wasEnabled != this.enabled)) EditorUtility.SetDirty(this); 
#endif
            
            ApplyState(enabled);
        }

        private void ApplyState(bool enabled) {
            for (int i = 0; i < _enableGroups?.Length; i++) {
                ref var group = ref _enableGroups[i];
                bool enable = enabled == (group.state == State.Enabled);
                
                for (int j = 0; j < group.objects.Length; j++) {
#if UNITY_EDITOR
                    if (!Application.isPlaying) {
                        if (group.objects[j] != null && group.objects[j].IsEnabled() != enable) {
                            group.objects[j].SetEnabled(enable);        
                            EditorUtility.SetDirty(group.objects[j]);  
                        }
                        continue;
                    }
#endif
                    
                    group.objects[j].SetEnabled(enable);
                }
            }
        }

        private bool GetMonoBehaviourEnableState() {
            return enabled && gameObject is { activeSelf: true, activeInHierarchy: true };
        }

#if UNITY_EDITOR
        [Header("Debug")]
        [SerializeField] private bool _applyStateInEditor = true;

        private byte _applyId;
        
        private void OnValidate() {
            if (!_applyStateInEditor || BuildInfo.IsBuildProcessing) return;

            bool enable = _mode switch {
                Mode.SyncMonoBehaviourEnable => GetMonoBehaviourEnableState(),
                Mode.Manual => _state == State.Enabled,
                _ => throw new ArgumentOutOfRangeException()
            };

            ApplyStateNextFrame(enable).Forget();
        }

        private async UniTask ApplyStateNextFrame(bool enable) {
            byte id = ++_applyId;
            await UniTask.Yield();
            
            if (id == _applyId) SetStateInternal(enable, notifyMonoBehaviour: false);
        }
#endif
    }
    
}