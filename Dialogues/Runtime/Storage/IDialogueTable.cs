using System.Collections.Generic;
using MisterGames.Common.Localization;
using MisterGames.Dialogues.Storage;

namespace MisterGames.Dialogues.Core {
    
    public interface IDialogueTable {

        LocalizationKey DialogueId { get; }
        
        IReadOnlyList<LocalizationKey> Roles { get; }
        IReadOnlyList<LocalizationKey> Branches { get; }
        IReadOnlyList<DialogueElement> Elements { get; }
        
        DialogueElement GetElementData(LocalizationKey elementKey);

    }
    
}