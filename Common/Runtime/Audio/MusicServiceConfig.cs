using UnityEngine;

namespace MisterGames.Common.Audio {
    
    [CreateAssetMenu(fileName = nameof(MusicServiceConfig), menuName = "MisterGames/Audio/" + nameof(MusicServiceConfig))]
    public sealed class MusicServiceConfig : ScriptableObject {

        [Min(0f)] public float fadeIn = 1f;
        [Min(0f)] public float fadeOut = 2f;
    }
    
}