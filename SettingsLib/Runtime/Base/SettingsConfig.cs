using UnityEngine;

namespace MisterGames.SettingsLib.Base {

    [CreateAssetMenu(fileName = nameof(SettingsConfig), menuName = "MisterGames/Settings/" + nameof(SettingsConfig))]
    public sealed class SettingsConfig : ScriptableObject {

        [Min(0f)] public float saveDirtyChangesTimeout = 2f;

    }
    
}