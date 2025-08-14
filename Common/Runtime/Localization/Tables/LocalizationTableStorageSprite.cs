using UnityEngine;

namespace MisterGames.Common.Localization {

    [CreateAssetMenu(fileName = nameof(LocalizationTableStorageSprite), menuName = "MisterGames/Localization/" + nameof(LocalizationTableStorageSprite))]
    public sealed class LocalizationTableStorageSprite : LocalizationTableStorageT<Sprite> { }
    
}