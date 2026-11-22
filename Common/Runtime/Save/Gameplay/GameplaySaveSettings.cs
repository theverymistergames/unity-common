using UnityEngine;

namespace MisterGames.Common.Save {

    [CreateAssetMenu(fileName = nameof(GameplaySaveSettings), menuName = "MisterGames/Save/" + nameof(GameplaySaveSettings))]
    public sealed class GameplaySaveSettings : ScriptableObject {

        [SerializeField] private string _versionKey = "_gameplay_save_version";
        [SerializeField] private string _version = "1";
        [SerializeField] private string _defaultProfileName = "profile";
        [SerializeField] private string _sharedProfileName = "shared";
        [SerializeField] private bool _autoSave = true;
        [SerializeField] [Min(0f)] private float _autoSaveDelay = 1f;

        public string Version => _version;
        public string VersionKey => _versionKey;
        public string SharedProfileName => _sharedProfileName;

        public bool IsAutoSaveEnabled(out float delay) {
            delay = _autoSaveDelay;
            return _autoSave;
        }
        
        public string CreateProfileName(int index) {
            return $"{_defaultProfileName}_{index}";
        }

        public bool IsProfileName(string storageId) {
            return storageId == _sharedProfileName || storageId.StartsWith($"{_defaultProfileName}_");
        }
    }
    
}