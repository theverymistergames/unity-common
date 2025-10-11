using System;
using MisterGames.Common.Localization;

namespace MisterGames.Dialogues.Storage {
    
    [Serializable]
    public struct DialogueElement {
        [HideLocalizationTable] public LocalizationKey roleId;
        [HideLocalizationTable] public LocalizationKey branchId;
        [HideLocalizationTable] public LocalizationKey key;
    }
    
}