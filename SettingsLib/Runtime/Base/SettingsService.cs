using System;
using MisterGames.Common.Save;
using MisterGames.Common.Tick;
using UnityEngine;

namespace MisterGames.SettingsLib.Base {
    
    public sealed class SettingsService : ISettingsService, IDisposable, IUpdate {

        public bool HasUnsavedChanges { get; private set; }

        private SettingsConfig _settingsConfig;
        private SettingsStorage _settingsStorage;
        private ISaveSystem _saveSystem;
        private string _storageId;
        private float _lastDirtyTime;

        public void Initialize(SettingsConfig settingsConfig, SettingsStorage settingsStorage, ISaveSystem saveSystem, string storageId) {
            _settingsConfig = settingsConfig;
            _settingsStorage = settingsStorage;
            _saveSystem = saveSystem;
            _storageId = storageId;
            
            ForEachSetting(_settingsStorage, (self, desc, label) => desc.Initialize(self, label));
            
            PlayerLoopStage.LateUpdate.Subscribe(this);
        }

        public void Dispose() {
            ForEachSetting(_settingsStorage, (self, desc, label) => desc.Deinitialize(self, label));
            
            PlayerLoopStage.LateUpdate.Unsubscribe(this);
        }

        void IUpdate.OnUpdate(float dt) {
            CheckDirty();
        }

        private void ForEachSetting(SettingsStorage settingsStorage, Action<ISettingsService, ISettingDesc, string> action) {
            int arraysCount = settingsStorage.GetArraysCount();
            
            for (int i = 0; i < arraysCount; i++) {
                int labelsCount = settingsStorage.GetArrayLabelsCount(i);
                
                for (int j = 0; j < labelsCount; j++) {
                    int labelId = settingsStorage.GetLabelId(i, j);
                    if (!settingsStorage.TryGetData(labelId, out var setting)) continue;
                    
                    action.Invoke(this, setting, settingsStorage.GetLabel(labelId));
                }
            }
        }

        public bool TryGet<T>(string key, int index, out T data) {
            return _saveSystem.TryGet(_storageId, key, index, out data);
        }
        
        public bool Set<T>(string key, int index, T setting) {
            NotifyDirty();
            return _saveSystem.Set(_storageId, key, index, setting);
        }

        public void SaveSettings() {
            ResetDirty();
            _saveSystem.SaveIntoFile(_storageId);
        }

        public void RevertToLastSavedSettings() {
            ResetDirty();
            _saveSystem.LoadFromFile(_storageId);
        }

        public void RevertToDefaultSettings() {
            ResetDirty();
            _saveSystem.DeleteFile(_storageId);
        }

        private void CheckDirty() {
            if (!HasUnsavedChanges ||
                _lastDirtyTime < 0f ||
                Time.realtimeSinceStartup < _lastDirtyTime + _settingsConfig.saveDirtyChangesTimeout) 
            {
                return;
            }
            
            SaveSettings();
        }
        
        private void NotifyDirty() {
            _lastDirtyTime = Time.realtimeSinceStartup;
            HasUnsavedChanges = true;
        }

        private void ResetDirty() {
            _lastDirtyTime = -1f;
            HasUnsavedChanges = false;
        }
    }
    
}