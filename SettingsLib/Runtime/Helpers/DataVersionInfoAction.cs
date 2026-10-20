using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Common.GameObjects;
using MisterGames.Common.Service;
using MisterGames.SettingsLib.Base;
using UnityEngine;

namespace MisterGames.SettingsLib.Versioning {
    
    [Serializable]
    public sealed class DataVersionInfoAction : IActorAction {

        public DataVersionInfo dataVersionInfo;
        public Operation operation;
        
        public enum Operation {
            Save,
            Remove,
        }
        
        public UniTask Apply(IActor context, CancellationToken cancellationToken = default) {
            switch (operation) {
                case Operation.Save:
                    SaveData(context);
                    break;
                
                case Operation.Remove:
                    RemoveData(context);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            return UniTask.CompletedTask;
        }
        
        private bool SaveData(IActor ctx) {
            if (!Services.TryGet(out ISettingsService service)) {
                Debug.LogError($"{nameof(DataVersionInfoAction)}: service of type {nameof(ISettingsService)} not found, ctx [{ctx?.GameObject.GetPathInScene()}]");
                return false;
            }

            if (!ValidateDataVersionInfo(ctx, out string dataId, out string versionDescriptor)) {
                return false;
            }
            
            if (!service.Set(dataId, index: 0, versionDescriptor)) {
                Debug.LogError($"{nameof(DataVersionInfoAction)}: version for [{dataVersionInfo}] could not be saved, ctx [{ctx?.GameObject.GetPathInScene()}]");
                return false;
            }

            return true;
        }

        private bool RemoveData(IActor ctx) {
            if (!Services.TryGet(out ISettingsService service)) {
                Debug.LogError($"{nameof(DataVersionInfoAction)}: service of type {nameof(ISettingsService)} not found, ctx [{ctx?.GameObject.GetPathInScene()}]");
                return false;
            }

            return ValidateDataVersionInfo(ctx, out string dataId, out string _) && 
                   service.Remove<string>(dataId, index: 0);
        }

        private bool ValidateDataVersionInfo(IActor ctx, out string dataId, out string versionDescriptor) {
            dataId = null;
            versionDescriptor = null;

            if (dataVersionInfo == null) {
                Debug.LogError($"{nameof(DataVersionInfoAction)}: data version object is null, ctx [{ctx?.GameObject.GetPathInScene()}]");
                return false;
            }

            dataId = dataVersionInfo.GetDataId();
            if (string.IsNullOrWhiteSpace(dataId)) {
                Debug.LogError($"{nameof(DataVersionInfoAction)}: dataId for [{dataVersionInfo}] is empty, ctx [{ctx?.GameObject.GetPathInScene()}]");
                return false;
            }
            
            versionDescriptor = dataVersionInfo.GetStringVersion();
            if (string.IsNullOrWhiteSpace(versionDescriptor)) {
                Debug.LogError($"{nameof(DataVersionInfoAction)}: version for [{dataVersionInfo}] is empty, ctx [{ctx?.GameObject.GetPathInScene()}]");
                return false;
            }

            return true;
        }
    }
    
}