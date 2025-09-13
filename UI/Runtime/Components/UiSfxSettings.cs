using MisterGames.Common.Attributes;
using UnityEngine;
using UnityEngine.Audio;

namespace MisterGames.UI.Components {

    [CreateAssetMenu(fileName = nameof(UiSfxSettings), menuName = "MisterGames/UI/" + nameof(UiSfxSettings))]
    public sealed class UiSfxSettings : ScriptableObject {

        [Header("Sound Settings")]
        [MinMaxSlider(0f, 2f)] public Vector2 volume = new Vector2(1f, 1f);
        [MinMaxSlider(0f, 2f)] public Vector2 pitch = new Vector2(0.9f, 1.1f);
        public bool affectedByTimeScale = true;
        public AudioMixerGroup mixerGroup;
        
    }
    
}