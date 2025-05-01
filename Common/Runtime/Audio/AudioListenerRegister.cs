using MisterGames.Common.Audio;
using MisterGames.Common.Labels;
using UnityEngine;

namespace MisterGames.Character.Behaviours {
    
    public sealed class AudioListenerRegister : MonoBehaviour {

        [SerializeField] private AudioListener _audioListener;
        [SerializeField] private Transform _up;
        [SerializeField] private LabelValue _priority;
        
        private void OnEnable() {
            AudioPool.Main?.RegisterListener(_audioListener, _up, _priority.GetValue());
        }

        private void OnDisable() {
            AudioPool.Main?.UnregisterListener(_audioListener);
        }

#if UNITY_EDITOR
        private void Reset() {
            _audioListener = GetComponent<AudioListener>();
            _up = transform;
        }
#endif
    }
    
}