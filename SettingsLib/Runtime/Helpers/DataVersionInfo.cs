using System;
using MisterGames.Common.Attributes;
using UnityEngine;

namespace MisterGames.SettingsLib.Versioning {

    [CreateAssetMenu(fileName = nameof(DataVersionInfo), menuName = "MisterGames/Settings/" + nameof(DataVersionInfo))]
    public sealed class DataVersionInfo : ScriptableObject {
        
        [SerializeField] private string _dataId;
        [SerializeField] private Mode _mode = Mode.UseInteger;
        [VisibleIf(nameof(_mode), 0)]
        [SerializeField] [Min(0)] private int _versionInt;
        [VisibleIf(nameof(_mode), 1)]
        [SerializeField] private string _version;

        public enum State {
            NoSavedData,
            IncorrectSavedData,
            ActualSavedData,
        }
        
        private enum Mode {
            UseInteger,
            UseString,
        }
        
        public string GetDataId() {
            return _dataId;
        }

        public string GetStringVersion() {
            return _mode switch {
                Mode.UseInteger => _versionInt.ToString(),
                Mode.UseString => _version,
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
    
}