using System;
using MisterGames.Common.Save;

namespace MisterGames.SettingsLib.Base {
    
    public sealed class SettingsService : ISettingsService, IDisposable {

        public bool HasUnsavedChanges { get; private set; }
        
        private ISaveSystem _saveSystem;
        private SettingsStorage _settingsStorage;
        private string _storageId;

        public void Initialize(SettingsStorage settingsStorage, ISaveSystem saveSystem, string storageId) {
            _settingsStorage = settingsStorage;
            _saveSystem = saveSystem;
            _storageId = storageId;
            
            ForEachSetting(_settingsStorage, (self, desc, label) => desc.Initialize(self, label));
        }

        public void Dispose() {
            ForEachSetting(_settingsStorage, (self, desc, label) => desc.Deinitialize(self, label));
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
            HasUnsavedChanges = true;
            return _saveSystem.Set(_storageId, key, index, setting);
        }

        public void SaveSettings() {
            HasUnsavedChanges = false;
            _saveSystem.SaveIntoFile(_storageId);
        }

        public void RevertToLastSavedSettings() {
            HasUnsavedChanges = false;
            _saveSystem.LoadFromFile(_storageId);
        }

        public void RevertToDefaultSettings() {
            HasUnsavedChanges = false;
            _saveSystem.DeleteFile(_storageId);
        }
    }
    
}