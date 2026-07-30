using MisterGames.Common.Localization;
using MisterGames.Input.Icons;
using UnityEngine;

namespace MisterGames.SettingsLib.Base {
    
    [CreateAssetMenu(fileName = nameof(KeyBindingConfig), menuName = "MisterGames/UI/" + nameof(KeyBindingConfig))]
    public sealed class KeyBindingConfig : ScriptableObject {
        
        public InputIconsTable inputIcons;

        [Header("Modal Dialog Rebinding")]
        public LocalizationKey rebindingDialogTitle;
        public LocalizationKey rebindingDialogContent;
        public LocalizationKey rebindingDialogUsedAction;
        public LocalizationKey rebindingDialogOk;
        public LocalizationKey rebindingDialogCancel;
    }
    
}