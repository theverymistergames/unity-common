using System;
using System.Threading;
using MisterGames.Common.Audio;
using MisterGames.Common.Maths;
using UnityEngine;

namespace MisterGames.UI.Components {
    
    [RequireComponent(typeof(UiButton))]
    public sealed class UiButtonSfx : MonoBehaviour {

        [SerializeField] private UiSfxSettings _uiSfxSettings;
        [SerializeField] private Option[] _stateOptions;
        [SerializeField] private AudioClip[] _clickSounds;
        
        [Serializable]
        private struct Option {
            public UiButton.State state;
            public AudioClip[] sounds;
        }
        
        private UiButton _button;
        
        private void Awake() {
            _button = GetComponent<UiButton>();
        }

        private void OnEnable() {
            _button.OnStateChanged += OnStateChanged;
            _button.OnClicked += OnClicked;
        }

        private void OnDisable() {
            _button.OnStateChanged -= OnStateChanged;
            _button.OnClicked -= OnClicked;
        }

        private void OnClicked() {
            PlaySound(_clickSounds);
        }

        private void OnStateChanged(UiButton.State state) {
            for (int i = 0; i < _stateOptions.Length; i++) {
                ref var option = ref _stateOptions[i];
                if (option.state != state) continue;
                
                PlaySound(option.sounds);
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