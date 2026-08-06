using UnityEngine;

namespace MisterGames.Common.Save {

    [CreateAssetMenu(fileName = nameof(GameplaySaveSettings), menuName = "MisterGames/Save/" + nameof(GameplaySaveSettings))]
    public sealed class GameplaySaveSettings : ScriptableObject {

        [SerializeField] private string _defaultProfileName = "profile";
        [SerializeField] private bool _autoSave = true;
        [SerializeField] [Min(0f)] private float _autoSaveDelay = 1f;

        public bool IsAutoSaveEnabled(out float delay) {
            delay = _autoSaveDelay;
            return _autoSave;
        }
        
        public string CreateProfileName(int index) {
            return $"{_defaultProfileName}_{index}";
        }
    }
    
}