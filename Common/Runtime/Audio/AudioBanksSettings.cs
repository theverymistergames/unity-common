using UnityEngine;

namespace MisterGames.Common.Audio {
    
    [CreateAssetMenu(fileName = nameof(AudioBanksSettings), menuName = "MisterGames/Audio/" + nameof(AudioBanksSettings))]
    public sealed class AudioBanksSettings : ScriptableObject {

        [Min(0f)] public float unloadDelay = 10f;
    }
    
}