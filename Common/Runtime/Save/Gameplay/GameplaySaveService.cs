using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using MisterGames.Common.Save.Storages;
using MisterGames.Common.Save.Tables;
using MisterGames.Common.Strings;
using MisterGames.Common.Tick;
using UnityEngine;

namespace MisterGames.Common.Save {
    
    public sealed class GameplaySaveService : IGameplaySaveService, IDisposable, IUpdate {

        public event Action<string> OnDataChanged = delegate { };
        public event Action<string> OnProfileChanged = delegate { };

        public bool HasUnsavedChanges => _unsavedChangesInProfiles.Count > 0; 

        private readonly Dictionary<string, ISaveStorage<SaveKey>> _storageMap = new();
        private readonly HashSet<string> _savedProfiles = new();
        private readonly HashSet<string> _unsavedChangesInProfiles = new();
        
        private ISaveSystem _saveSystem;
        private GameplaySaveSettings _settings;
        private string _currentProfileKey;
        private float _lastDirtyTime = -1f;

        public void Initialize(GameplaySaveSettings settings, ISaveSystem saveSystem) {
            _settings = settings;
            _saveSystem = saveSystem;
            
            FetchProfiles();
            
            PlayerLoopStage.LateUpdate.Subscribe(this);
        }

        public void Dispose() {
            PlayerLoopStage.LateUpdate.Unsubscribe(this);
        }

        void IUpdate.OnUpdate(float dt) {
            if (!_settings.IsAutoSaveEnabled(out float delay) || 
                _lastDirtyTime >= 0f && Time.realtimeSinceStartup < _lastDirtyTime + delay ||
                GetCurrentProfileKey() is not { } currentProfileKey || 
                !_unsavedChangesInProfiles.Contains(currentProfileKey)) 
            {
                return;
            }

            _lastDirtyTime = -1f;
            SaveProfile(currentProfileKey).Forget();
        }

        public string GetCurrentProfileKey() {
            return _currentProfileKey;
        }

        public string GetProfileKey(int index) {
            return _settings.CreateProfileName(index);
        }

        public async UniTask LoadOrCreateProfile(string profileKey) {
            _unsavedChangesInProfiles.Remove(profileKey);
            await _saveSystem.LoadFromFile(profileKey, GetOrCreateStorage(profileKey));
            _currentProfileKey = profileKey;
            FetchProfiles();
            OnProfileChanged.Invoke(profileKey);
        }
        
        public async UniTask SaveProfile(string profileKey) {
            _unsavedChangesInProfiles.Remove(profileKey);
            await _saveSystem.SaveIntoFile(profileKey, GetOrCreateStorage(profileKey));
            FetchProfiles();
        }
        
        public void DeleteProfile(string profileKey) {
            _unsavedChangesInProfiles.Remove(profileKey);
            _saveSystem.DeleteFile(profileKey);
            _storageMap.Remove(profileKey);
            FetchProfiles();
        }

        public bool HasSavedProfile(string profileKey) {
            return _savedProfiles.Contains(profileKey);
        }

        private void FetchProfiles() {
            _savedProfiles.Clear();
            var storageFiles = _saveSystem.GetStorageFiles();
            for (int i = 0; i < storageFiles.Count; i++) {
                _savedProfiles.Add(storageFiles[i].storageId);
            }
        }
        
        public bool TryGet<T>(string key, int index, out T data) {
            data = default;
            return GetOrCreateStorage(_currentProfileKey)?.GetTable<T>()?.TryGetData(new SaveKey(key, index), out data) ?? false;
        }
        
        public bool Set<T>(string key, int index, T setting) {
            bool ok = GetOrCreateStorage(_currentProfileKey)?.GetOrCreateTable<T>().SetData(new SaveKey(key, index), setting) ?? false;
            if (ok) {
                _unsavedChangesInProfiles.Add(_currentProfileKey);
                _lastDirtyTime = Time.realtimeSinceStartup;
                OnDataChanged.Invoke(key);
            }
            return ok;
        }

        public bool Remove<T>(string key, int index) {
            bool ok = GetOrCreateStorage(_currentProfileKey)?.GetTable<T>()?.RemoveData(new SaveKey(key, index)) ?? false;
            if (ok) {
                _unsavedChangesInProfiles.Add(_currentProfileKey);
                _lastDirtyTime = Time.realtimeSinceStartup;
                OnDataChanged.Invoke(key);
            }
            return ok;
        }

        private ISaveStorage<SaveKey> GetOrCreateStorage(string profileKey) {
            if (string.IsNullOrWhiteSpace(_currentProfileKey)) {
                Debug.LogError($"{nameof(GameplaySaveService).FormatColorOnlyForEditor(Color.white)}: {Time.frameCount}, " +
                               $"trying to get or create storage for empty profile name");
                return null;
            }
            
            if (!_storageMap.TryGetValue(profileKey, out var storage)) {
                storage = new SaveStorage<SaveKey>();
                _storageMap[profileKey] = storage;
            }
            
            return storage;
        }
    }
    
}