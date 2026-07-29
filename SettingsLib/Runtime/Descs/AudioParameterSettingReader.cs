using System;
using UnityEngine.Audio;

namespace MisterGames.SettingsLib.Descs {
    
    [Serializable]
    public sealed class AudioParameterSettingReader : ISettingReaderFloat {
    
        public AudioMixer audioMixer;
        public string[] parameters;
        
        public void OnReadValue(float value) {
            for (int i = 0; i < parameters.Length; i++) {
                audioMixer.SetFloat(parameters[i], value);
            }
        }
    }
    
}