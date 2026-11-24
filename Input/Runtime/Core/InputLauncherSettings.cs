using System;
using MisterGames.Input.Actions;
using UnityEngine;

namespace MisterGames.Input.Core {

    [CreateAssetMenu(fileName = nameof(InputLauncherSettings), menuName = "MisterGames/Input/" + nameof(InputLauncherSettings))]
    public sealed class InputLauncherSettings : ScriptableObject {

        public bool enableLogs;
        public InputActionRef[] ignoreInitialStateCheckForInputs;

        internal event Action OnValidated = delegate { };
        
        private void OnValidate() {
            OnValidated.Invoke();
        }
    }
    
}