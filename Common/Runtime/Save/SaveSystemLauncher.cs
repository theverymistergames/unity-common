using System;
using MisterGames.Common.Attributes;
using UnityEngine;

namespace MisterGames.Common.Save {
    
    [DefaultExecutionOrder(-100_000)]
    public sealed class SaveSystemLauncher : MonoBehaviour {

        [EmbeddedInspector]
        [SerializeField] private SaveSystemSettings _saveSystemSettings;

        private void Awake() {
            ((SaveSystem) SaveSystem.Main).Initialize(_saveSystemSettings);
        }

        private void OnDestroy() {
            ((IDisposable) SaveSystem.Main).Dispose();
        }
    }
    
}