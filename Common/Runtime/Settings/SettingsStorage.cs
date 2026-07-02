using MisterGames.Common.Save.Tables;
using UnityEngine;

namespace MisterGames.Common.Settings {

    [CreateAssetMenu(fileName = nameof(SettingsStorage), menuName = "MisterGames/" + nameof(SettingsStorage))]
    public sealed class SettingsStorage : ScriptableObject {

        [SerializeField] private SaveStorage _saveStorage;

    }
    
}