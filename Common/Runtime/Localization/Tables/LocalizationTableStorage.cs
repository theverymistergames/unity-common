using UnityEngine;

namespace MisterGames.Common.Localization {

    [CreateAssetMenu(fileName = nameof(LocalizationTableStorage), menuName = "MisterGames/Localization/" + nameof(LocalizationTableStorage))]
    public sealed class LocalizationTableStorage : LocalizationTableStorageT<string> { }
    
}