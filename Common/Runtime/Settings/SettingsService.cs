using System;
using MisterGames.Common.Save;

namespace MisterGames.Common.Settings {
    
    public sealed class SettingsService : IDisposable, ISaveable {

        private ISaveSystem _saveSystem;
        private string _storageId;
        
        public void Initialize(ISaveSystem saveSystem, string storageId) {
            _saveSystem = saveSystem;
            _storageId = storageId;
            
            _saveSystem.Register(this, notifyLoad: true);
        }

        public void Dispose() {
            _saveSystem.Unregister(this);
        }

        public void OnLoadData(ISaveSystem saveSystem) {
            
        }

        public void OnSaveData(ISaveSystem saveSystem) {
            
        }

        public SaveBuilder PopData<T>(string key, out T data) {
            return _saveSystem.Pop(_storageId, key, out data);
        }

        public SaveBuilder PushData<T>(string key, T setting) {
            return _saveSystem.Push(_storageId, key, setting);
        }

        public void SaveSettings() {
            _saveSystem.SaveIntoFile(_storageId);
        }

        public void RevertToLastSavedSettings() {
            _saveSystem.LoadFromFile(_storageId);
        }

        public void RevertToDefaultSettings() {
            _saveSystem.DeleteFile(_storageId);
        }
    }
    
}