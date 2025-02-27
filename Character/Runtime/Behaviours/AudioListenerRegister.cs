using MisterGames.Actors;
using MisterGames.Common.Audio;
using UnityEngine;

namespace MisterGames.Character.Behaviours {
    
    public sealed class AudioListenerRegister : MonoBehaviour, IActorComponent {
        
        private AudioListener _audioListener;
        
        void IActorComponent.OnAwake(IActor actor) {
            _audioListener = actor.GetComponent<AudioListener>();
            AudioPool.Main?.RegisterListener(_audioListener, actor.Transform);
        }

        void IActorComponent.OnDestroyed(IActor actor) {
            AudioPool.Main?.UnregisterListener(_audioListener);
        }
    }
    
}