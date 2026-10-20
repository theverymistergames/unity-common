using System;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Common.GameObjects;
using MisterGames.Common.Service;
using MisterGames.SettingsLib.Base;
using UnityEngine;

namespace MisterGames.SettingsLib.Versioning {
    
    [Serializable]
    public sealed class DataVersionInfoCondition : IActorCondition {

        public DataVersionInfo dataVersionInfo;
        public bool shouldHaveActualVersion = true;
        
        public bool IsMatch(IActor context) {
            return FetchState(context) is DataVersionInfo.State.ActualSavedData == shouldHaveActualVersion;
        }
        
        private DataVersionInfo.State FetchState(IActor ctx) {
            if (!Services.TryGet(out ISettingsService service)) {
                Debug.LogError($"{nameof(DataVersionInfoCondition)}: service of type {nameof(ISettingsService)} not found, ctx [{ctx?.GameObject?.GetPathInScene()}]");
                return DataVersionInfo.State.NoSavedData;
            }
            
            if (!ValidateDataVersionInfo(ctx, out string dataId, out string versionDescriptor)) {
                return DataVersionInfo.State.NoSavedData;
            }
            
            if (!service.TryGet(dataId, index: 0, out string savedVersionDescriptor)) {
                return DataVersionInfo.State.NoSavedData;    
            }

            return savedVersionDescriptor == versionDescriptor 
                ? DataVersionInfo.State.ActualSavedData 
                : DataVersionInfo.State.IncorrectSavedData;
        }
        
        private bool ValidateDataVersionInfo(IActor ctx, out string dataId, out string versionDescriptor) {
            dataId = null;
            versionDescriptor = null;

            if (dataVersionInfo == null) {
                Debug.LogError($"{nameof(DataVersionInfoCondition)}: data version object is null, ctx [{ctx?.GameObject.GetPathInScene()}]");
                return false;
            }

            dataId = dataVersionInfo.GetDataId();
            if (string.IsNullOrWhiteSpace(dataId)) {
                Debug.LogError($"{nameof(DataVersionInfoCondition)}: dataId for [{dataVersionInfo}] is empty, ctx [{ctx?.GameObject.GetPathInScene()}]");
                return false;
            }
            
            versionDescriptor = dataVersionInfo.GetStringVersion();
            if (string.IsNullOrWhiteSpace(versionDescriptor)) {
                Debug.LogError($"{nameof(DataVersionInfoCondition)}: version for [{dataVersionInfo}] is empty, ctx [{ctx?.GameObject.GetPathInScene()}]");
                return false;
            }

            return true;
        }
    }
    
}