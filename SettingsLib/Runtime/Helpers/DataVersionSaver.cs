using System;
using MisterGames.Common.GameObjects;
using MisterGames.Common.Service;
using MisterGames.SettingsLib.Base;
using UnityEngine;

namespace MisterGames.SettingsLib.Versioning {
    
    [DefaultExecutionOrder(-1000)]
    public sealed class DataVersionSaver : MonoBehaviour {
        
        [SerializeField] private DataVersionInfo _dataVersionInfo;
        
        public DataVersionInfo.State CurrentState { get; private set; }
        public event Action<DataVersionInfo.State> OnStateChanged = delegate { };

        private bool _isValidated;
        private string _dataId;
        private string _versionDescriptor;
        
        private void Awake() {
            CurrentState = FetchState(forceValidation: true);
            OnStateChanged.Invoke(CurrentState);
            
            SubscribeDataChange();
        }

        private void OnDestroy() {
            UnsubscribeDataChange();
        }

        public void SaveVersion() {
            SaveData();
        }

        public void RemoveVersion() {
            RemoveData();
        }

        private void SubscribeDataChange() {
            if (!Services.TryGet(out ISettingsService service)) return;
            
            service.OnDataChanged += OnDataChanged;
        }
        
        private void UnsubscribeDataChange() {
            if (!Services.TryGet(out ISettingsService service)) return;
            
            service.OnDataChanged -= OnDataChanged;
        }

        private void OnDataChanged(string key) {
            if (!ValidateDataVersionInfo(forceValidation: !_isValidated, out string dataId, out _) || 
                dataId != key) 
            {
                return;
            }
            
            var oldState = CurrentState;
            CurrentState = FetchState(forceValidation: false);
            
            if (oldState != CurrentState) OnStateChanged.Invoke(CurrentState);
        }

        private DataVersionInfo.State FetchState(bool forceValidation) {
            if (!Services.TryGet(out ISettingsService service)) {
                Debug.LogError($"{nameof(DataVersionSaver)}: service of type {nameof(ISettingsService)} not found on [{gameObject.GetPathInScene()}]");
                return DataVersionInfo.State.NoSavedData;
            }
            
            if (!ValidateDataVersionInfo(forceValidation, out string dataId, out string versionDescriptor)) {
                return DataVersionInfo.State.NoSavedData;
            }
            
            if (!service.TryGet(dataId, index: 0, out string savedVersionDescriptor)) {
                return DataVersionInfo.State.NoSavedData;    
            }

            return savedVersionDescriptor == versionDescriptor 
                ? DataVersionInfo.State.ActualSavedData 
                : DataVersionInfo.State.IncorrectSavedData;
        }

        private bool SaveData() {
            if (!Services.TryGet(out ISettingsService service)) {
                Debug.LogError($"{nameof(DataVersionSaver)}: service of type {nameof(ISettingsService)} not found on [{gameObject.GetPathInScene()}]");
                return false;
            }

            if (!ValidateDataVersionInfo(forceValidation: true, out string dataId, out string versionDescriptor)) {
                return false;
            }
            
            if (!service.Set(dataId, index: 0, versionDescriptor)) {
                Debug.LogError($"{nameof(DataVersionSaver)}: version for [{_dataVersionInfo}] could not be saved on [{gameObject.GetPathInScene()}]");
                return false;
            }
            
            return true;
        }

        private bool RemoveData() {
            if (!Services.TryGet(out ISettingsService service)) {
                Debug.LogError($"{nameof(DataVersionSaver)}: service of type {nameof(ISettingsService)} not found on [{gameObject.GetPathInScene()}]");
                return false;
            }

            return ValidateDataVersionInfo(forceValidation: true, out string dataId, out _) && 
                   service.Remove<string>(dataId, index: 0);
        }

        private bool ValidateDataVersionInfo(bool forceValidation, out string dataId, out string versionDescriptor) {
            dataId = null;
            versionDescriptor = null;

            if (!forceValidation && _isValidated) {
                dataId = _dataId;
                versionDescriptor = _versionDescriptor;
                
                return !string.IsNullOrWhiteSpace(_dataId) && 
                       !string.IsNullOrWhiteSpace(_versionDescriptor);
            }
            
            _isValidated = true;
            
            if (_dataVersionInfo == null) {
                Debug.LogError($"{nameof(DataVersionSaver)}: data version object is null on [{gameObject.GetPathInScene()}]");
                return false;
            }

            dataId = _dataVersionInfo.GetDataId();
            _dataId = dataId;
            
            if (string.IsNullOrWhiteSpace(dataId)) {
                Debug.LogError($"{nameof(DataVersionSaver)}: dataId for [{_dataVersionInfo}] is empty on [{gameObject.GetPathInScene()}]");
                return false;
            }
            
            versionDescriptor = _dataVersionInfo.GetStringVersion();
            _versionDescriptor = versionDescriptor;
            
            if (string.IsNullOrWhiteSpace(versionDescriptor)) {
                Debug.LogError($"{nameof(DataVersionSaver)}: version for [{_dataVersionInfo}] is empty on [{gameObject.GetPathInScene()}]");
                return false;
            }

            return true;
        }
    }
    
}