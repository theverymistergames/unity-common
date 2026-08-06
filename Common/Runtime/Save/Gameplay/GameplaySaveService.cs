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

        public event IGameplaySaveService.DataCallback OnDataChanged = delegate { };
        public event IGameplaySaveService.ProfileCallback OnCurrentProfileChanged = delegate { };
        public event IGameplaySaveService.ProfileCallback OnProfileUpdated = delegate { };
        
        public bool HasUnsavedChanges => _unsavedChangesInProfiles.Count > 0; 

        private readonly Dictionary<string, ISaveStorage<SaveKey>> _storageMap = new();
        private readonly Dictionary<string, float> _unsavedChangesInProfiles = new();
        private readonly HashSet<string> _savedProfiles = new();
        private readonly HashSet<string> _dirtyBuffer = new();

        private ISaveSystem _saveSystem;
        private GameplaySaveSettings _settings;
        private string _currentProfileKey;

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
            if (!_settings.IsAutoSaveEnabled(out float delay)) return;

            float time = Time.realtimeSinceStartup;
            _dirtyBuffer.Clear();
            
            foreach ((string profile, float lastDirtyTime) in _unsavedChangesInProfiles) {
                if (time < lastDirtyTime + delay) continue;
                
                _dirtyBuffer.Add(profile);
            }
            
            foreach (string profile in _dirtyBuffer) {
                SaveProfile(profile).Forget();   
            }
        }

        public string GetCurrentProfileKey() {
            return _currentProfileKey;
        }

        public string GetProfileKey(int index) {
            return _settings.CreateProfileName(index);
        }

        public async UniTask LoadOrCreateProfile(string profileKey, bool makeCurrent) {
            _unsavedChangesInProfiles.Remove(profileKey);
            if (makeCurrent) _currentProfileKey = profileKey;
            
            await _saveSystem.LoadFromFile(profileKey, GetOrCreateStorage(profileKey));
            
            FetchProfiles();
            
            OnProfileUpdated.Invoke(profileKey);
            if (makeCurrent) OnCurrentProfileChanged.Invoke(profileKey);
        }
        
        public async UniTask SaveProfile(string profileKey) {
            _unsavedChangesInProfiles.Remove(profileKey);
            
            if (GetStorage(profileKey) is { } storage) {
                await _saveSystem.SaveIntoFile(profileKey, storage);    
            }
            
            FetchProfiles();
        }
        
        public void DeleteProfile(string profileKey) {
            _unsavedChangesInProfiles.Remove(profileKey);
            _savedProfiles.Remove(profileKey);
            _saveSystem.DeleteFile(profileKey);
            _storageMap.Remove(profileKey);
            OnProfileUpdated.Invoke(profileKey);
            FetchProfiles();
        }

        public bool HasSavedProfile(string profileKey) {
            return _savedProfiles.Contains(profileKey);
        }

        public bool IsProfileLoaded(string profileKey) {
            return _storageMap.ContainsKey(profileKey);
        }

        private void FetchProfiles() {
            _savedProfiles.Clear();
            var storageFiles = _saveSystem.GetStorageFiles();
            for (int i = 0; i < storageFiles.Count; i++) {
                _savedProfiles.Add(storageFiles[i].storageId);
            }
        }
        
        public bool TryGet<T>(string profileKey, string key, int index, out T data) {
            data = default;
            
            if (string.IsNullOrWhiteSpace(key)) {
                Debug.LogError($"{nameof(GameplaySaveService).FormatColorOnlyForEditor(Color.white)}: {Time.frameCount}, " +
                               $"trying to get data of type {typeof(T)} with empty key");
                return false;
            }
            
            if (string.IsNullOrWhiteSpace(profileKey)) {
                Debug.LogError($"{nameof(GameplaySaveService).FormatColorOnlyForEditor(Color.white)}: {Time.frameCount}, " +
                               $"trying to get data of type {typeof(T)} with key {key} for empty profile");
                return false;
            }
            
            return GetStorage(profileKey)?.GetTable<T>()?.TryGetData(new SaveKey(key, index), out data) ?? false;
        }
        
        public bool Set<T>(string profileKey, string key, int index, T data) {
            if (string.IsNullOrWhiteSpace(key)) {
                Debug.LogError($"{nameof(GameplaySaveService).FormatColorOnlyForEditor(Color.white)}: {Time.frameCount}, " +
                               $"trying to set data of type {typeof(T)} with empty key");
                return false;
            }
            
            if (string.IsNullOrWhiteSpace(profileKey)) {
                Debug.LogError($"{nameof(GameplaySaveService).FormatColorOnlyForEditor(Color.white)}: {Time.frameCount}, " +
                               $"trying to set data of type {typeof(T)} with key {key} for empty profile");
                return false;
            }
            
            bool ok = GetStorage(profileKey)?.GetOrCreateTable<T>().SetData(new SaveKey(key, index), data) ?? false;
            
            if (ok) {
                _unsavedChangesInProfiles[profileKey] = Time.realtimeSinceStartup;
                OnDataChanged.Invoke(profileKey, key);
            }
            
            return ok;
        }

        public bool Remove<T>(string profileKey, string key, int index) {
            if (string.IsNullOrWhiteSpace(key)) {
                Debug.LogError($"{nameof(GameplaySaveService).FormatColorOnlyForEditor(Color.white)}: {Time.frameCount}, " +
                               $"trying to set data of type {typeof(T)} with empty key");
                return false;
            }
            
            if (string.IsNullOrWhiteSpace(profileKey)) {
                Debug.LogError($"{nameof(GameplaySaveService).FormatColorOnlyForEditor(Color.white)}: {Time.frameCount}, " +
                               $"trying to remove data of type {typeof(T)} with key {key} for empty profile");
                return false;
            }
            
            bool ok = GetStorage(profileKey)?.GetTable<T>()?.RemoveData(new SaveKey(key, index)) ?? false;
            
            if (ok) {
                _unsavedChangesInProfiles[profileKey] = Time.realtimeSinceStartup;
                OnDataChanged.Invoke(profileKey, key);
            }
            
            return ok;
        }
        
        public bool TryGet<T>(string key, int index, out T data) {
            return TryGet(_currentProfileKey, key, index, out data);
        }
        
        public bool Set<T>(string key, int index, T data) {
            return Set(_currentProfileKey, key, index, data);
        }

        public bool Remove<T>(string key, int index) {
            return Remove<T>(_currentProfileKey, key, index);
        }

        private ISaveStorage<SaveKey> GetOrCreateStorage(string profileKey) {
            if (string.IsNullOrWhiteSpace(profileKey)) {
                Debug.LogError($"{nameof(GameplaySaveService).FormatColorOnlyForEditor(Color.white)}: {Time.frameCount}, " +
                               "trying to get or create storage for empty profile name");
                return null;
            }
            
            if (!_storageMap.TryGetValue(profileKey, out var storage)) {
                storage = new SaveStorage<SaveKey>();
                _storageMap[profileKey] = storage;
            }
            
            return storage;
        }
        
        private ISaveStorage<SaveKey> GetStorage(string profileKey) {
            if (string.IsNullOrWhiteSpace(profileKey)) {
                Debug.LogError($"{nameof(GameplaySaveService).FormatColorOnlyForEditor(Color.white)}: {Time.frameCount}, " +
                               "trying to get storage for empty profile name");
                return null;
            }
            
            return _storageMap.GetValueOrDefault(profileKey);
        }
    }
    
}