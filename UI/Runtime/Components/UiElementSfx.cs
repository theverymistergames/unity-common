using System;
using System.Threading;
using MisterGames.Common.Attributes;
using MisterGames.Common.Audio;
using MisterGames.Common.Labels;
using MisterGames.Common.Maths;
using MisterGames.UI.Data;
using UnityEngine;

namespace MisterGames.UI.Components {
    
    [RequireComponent(typeof(UiElementAnimator))]
    public sealed class UiElementSfx : MonoBehaviour {

        [EmbeddedInspector]
        [SerializeField] private UiSfxSettings _uiSfxSettings;
        [SerializeField] private Option[] _stateOptions;

        [Serializable]
        private struct Option {
            public UiElementState state;
            public LabelValue<AudioClip[]> sounds;
        }
        
        private IUiElementAnimator _animator;
        
        private void Awake() {
            _animator = GetComponent<IUiElementAnimator>();
        }

        private void OnEnable() {
            if (_animator != null) _animator.OnStateChanged += OnStateChanged;
        }

        private void OnDisable() {
            if (_animator != null) _animator.OnStateChanged -= OnStateChanged;
        }

        private void OnStateChanged(UiElementState state) {
            for (int i = 0; i < _stateOptions.Length; i++) {
                ref var option = ref _stateOptions[i];
                if (option.state != state) continue;
                
                PlaySound(option.sounds.GetData());
                break;
            }
        }

        private void PlaySound(AudioClip[] sounds) {
            if (sounds is not { Length: > 0 } || AudioPool.Main is not { } audioPool) return;

            audioPool.Play(
                audioPool.ShuffleClips(sounds),
                Vector3.zero,
                _uiSfxSettings.volume.GetRandomInRange(),
                pitch: _uiSfxSettings.pitch.GetRandomInRange(),
                spatialBlend: 0f,
                mixerGroup: _uiSfxSettings.mixerGroup,
                options: _uiSfxSettings.affectedByTimeScale ? AudioOptions.AffectedByTimeScale : AudioOptions.None,
                cancellationToken: CancellationToken.None
            );
        }
    }
    
}