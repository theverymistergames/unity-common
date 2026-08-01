using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Common.Async;
using MisterGames.Common.Save;
using MisterGames.Common.Save.Storages;
using MisterGames.Common.Save.Tables;
using MisterGames.Common.Strings;
using MisterGames.Common.Tick;
using UnityEngine;

namespace MisterGames.SettingsLib.Base {
    
    public sealed class SettingsService : ISettingsService, IDisposable, IUpdate {

        public bool HasUnsavedChanges => _dirtySet.Count > 0;

        private readonly Dictionary<string, ISettingDesc> _settingMap = new();
        private readonly HashSet<int> _dirtySet = new();
        private readonly ISaveStorage<SaveKey> _saveStorage = new SaveStorage<SaveKey>();
        private ISaveStorage<SaveKey> _tempStorage;
        private HashSet<string> _tempStorageKeys;

        private CancellationTokenSource _cts;
        private SettingsConfig _settingsConfig;
        private SettingsStorage _settingsStorage;
        private ISaveSystem _saveSystem;
        private string _storageId;
        private float _lastDirtyTime;

        public void Initialize(SettingsConfig settingsConfig, ISaveSystem saveSystem) {
            AsyncExt.RecreateCts(ref _cts);
            
            _settingsConfig = settingsConfig;
            _settingsStorage = settingsConfig.settingsStorage;
            _saveSystem = saveSystem;
            _storageId = settingsConfig.storageId;

            FetchSettings(_settingsStorage);
            InitializeAsync(_cts.Token).Forget();
        }

        public void Dispose() {
            AsyncExt.DisposeCts(ref _cts);
            
            foreach ((string key, var desc) in _settingMap) {
                desc.Deinitialize(this, key);
            }
            
            _saveStorage.Clear();
            _tempStorage?.Clear();
            
            PlayerLoopStage.LateUpdate.Unsubscribe(this);
        }

        private async UniTask InitializeAsync(CancellationToken cancellationToken) {
            await _saveSystem.LoadFromFile(_storageId, _saveStorage);
            if (cancellationToken.IsCancellationRequested) return;
            
            foreach ((string key, var desc) in _settingMap) {
                desc.Initialize(this, key);
                desc.ApplySetting(this, key);
            }
            
            PlayerLoopStage.LateUpdate.Subscribe(this);
        }

        void IUpdate.OnUpdate(float dt) {
            ProcessAutoSave();
        }

        public bool TryGet<T>(string key, int index, out T data) {
            data = default;
            
            var storage = _tempStorage != null && _tempStorageKeys != null && _tempStorageKeys.Contains(key)
                ? _tempStorage
                : _saveStorage;
            
            return storage.GetTable<T>()?.TryGetData(new SaveKey(key, index), out data) ?? false;
        }
        
        public bool Set<T>(string key, int index, T setting) {
            NotifyDirty(key);
            return _saveStorage.GetOrCreateTable<T>()?.SetData(new SaveKey(key, index), setting) ?? false;
        }

        public bool Remove<T>(string key, int index) {
            NotifyDirty(key);
            return _saveStorage.GetTable<T>()?.RemoveData(new SaveKey(key, index)) ?? false;
        }

        public void SaveSettings() {
            ResetDirty();
            _saveSystem.SaveIntoFile(_storageId, _saveStorage);
        }

        public void RevertToDefaultSettings() {
            ResetDirty();
            
            _saveStorage.Clear();
            _saveSystem.DeleteFile(_storageId);
            
            foreach ((string key, var desc) in _settingMap) {
                desc.ClearSetting(this, key);
                desc.ApplySetting(this, key);
            }
        }

        public void RevertToLastSavedSettings() {
            ResetDirty();
            
            _saveStorage.Clear();
            _saveSystem.GetStorage(_storageId)?.CopyTo(_saveStorage);

            foreach ((string key, var desc) in _settingMap) {
                desc.ApplySetting(this, key);
            }
        }

        public void RevertToDefaultSettings(HashSet<string> keys) {
            ResetDirty(keys);
            
            foreach (string key in keys) {
                if (!_settingMap.TryGetValue(key, out var desc)) continue;
                
                desc.ClearSetting(this, key);
                desc.ApplySetting(this, key);
            }
        }

        public void RevertToLastSavedSettings(HashSet<string> keys) {
            ResetDirty(keys);

            _tempStorageKeys = keys;
            _tempStorage = _saveSystem.GetStorage(_storageId);
            
            foreach (string key in keys) {
                if (!_settingMap.TryGetValue(key, out var desc)) continue;
                
                desc.ResaveSetting(this, key);
                desc.ApplySetting(this, key);
            }

            _tempStorage.Clear();
            _tempStorage = null;
            _tempStorageKeys = null;
        }

        private void FetchSettings(SettingsStorage settingsStorage) {
            int arraysCount = settingsStorage.GetArraysCount();
            
            for (int i = 0; i < arraysCount; i++) {
                int labelsCount = settingsStorage.GetArrayLabelsCount(i);
                
                for (int j = 0; j < labelsCount; j++) {
                    int labelId = settingsStorage.GetLabelId(i, j);
                    if (!settingsStorage.TryGetData(labelId, out var setting)) continue;

                    string key = settingsStorage.GetFullLabel(labelId);
                    if (_settingMap.TryAdd(key, setting)) continue;
                    
                    Debug.LogError($"{nameof(SettingsService)}: already contains setting with key [{key}], duplicate keys are not supported.");
                }
            }
        }

        private void ProcessAutoSave() {
            if (!_settingsConfig.autosave ||
                !HasUnsavedChanges ||
                _lastDirtyTime < 0f ||
                Time.realtimeSinceStartup < _lastDirtyTime + _settingsConfig.saveDirtyChangesTimeout) 
            {
                return;
            }
            
            SaveSettings();
        }

        private void NotifyDirty(string key) {
            _lastDirtyTime = Time.realtimeSinceStartup;
            _dirtySet.Add(Animator.StringToHash(key));
        }

        private void ResetDirty(string key) {
            _dirtySet.Remove(Animator.StringToHash(key));
            if (_dirtySet.Count == 0) _lastDirtyTime = -1f;
        }

        private void ResetDirty(HashSet<string> keys) {
            foreach (string key in keys) {
                ResetDirty(key);
            }
        }
        
        private void ResetDirty() {
            _lastDirtyTime = -1f;
            _dirtySet.Clear();
        }
    }
    
}