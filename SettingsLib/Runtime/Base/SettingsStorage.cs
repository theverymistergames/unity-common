using MisterGames.Common.Labels;
using UnityEngine;

namespace MisterGames.SettingsLib.Base {

    [CreateAssetMenu(fileName = nameof(SettingsStorage), menuName = "MisterGames/Settings/" + nameof(SettingsStorage))]
    public sealed class SettingsStorage : LabelLibraryByRef<ISettingDesc> { }
    
}