using UnityEngine;

namespace MisterGames.SettingsLib.Base {

    [CreateAssetMenu(fileName = nameof(SettingsConfig), menuName = "MisterGames/Settings/" + nameof(SettingsConfig))]
    public sealed class SettingsConfig : ScriptableObject {
        
        public SettingsStorage settingsStorage;
        public string storageId = "GameSettings";
        
        [Header("Autosave")]
        public bool autosave = true;
        [Min(0f)] public float saveDirtyChangesTimeout = 2f;
        
    }
    
}