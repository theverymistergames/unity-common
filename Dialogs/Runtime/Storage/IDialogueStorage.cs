using MisterGames.Common.Localization;

namespace MisterGames.Dialogues.Storage {
    
    public interface IDialogueStorage {
    
        LocalizationKey DialogueId { get; }
        int RolesCount { get; }
        int BranchesCount { get; }
        int ElementsCount { get; }

        void SetDialogueId(LocalizationKey id);
        
        LocalizationKey GetRoleAt(int index);
        void AddRole(LocalizationKey role);
        
        LocalizationKey GetBranchAt(int index);
        void AddBranch(LocalizationKey branch);
        
        DialogueElement GetElementAt(int index);
        void AddElement(DialogueElement element);

        void ClearAll();
    }
    
}