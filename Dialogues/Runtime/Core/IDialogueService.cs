using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Common.Localization;
using MisterGames.Dialogues.Storage;

namespace MisterGames.Dialogues.Core {
    
    public interface IDialogueService {

        public delegate void DialogueGenericEvent(LocalizationKey dialogue, DialogueEvent type);
        public delegate void DialogueStart(LocalizationKey dialogue);
        public delegate void GroupStart(LocalizationKey dialogue, LocalizationKey branch, LocalizationKey role);
        public delegate void ElementStart(LocalizationKey dialogue, DialogueElement element);
        
        event DialogueStart OnDialogueStart;
        event DialogueStart OnDialogueStop;
        event GroupStart OnDialogueBranchStart;
        event GroupStart OnDialogueRoleStart;
        event ElementStart OnDialogueElementStart;
        event DialogueGenericEvent OnAnyDialogueEvent;
        
        IDialogueTable LoadDialogue(string guid);
        UniTask<IDialogueTable> LoadDialogueAsync(string guid);
        void UnloadDialogue(string guid);
        
        void StartDialogue(LocalizationKey dialogue);
        void StopDialogue(LocalizationKey dialogue);
        
        void StartDialogueElement(LocalizationKey dialogue, DialogueElement element);

        LocalizationKey GetBranch(LocalizationKey dialogue);
        LocalizationKey GetRole(LocalizationKey dialogue);
        DialogueElement GetElement(LocalizationKey dialogue);
        
        void AddDialogueEvent(LocalizationKey key, DialogueEvent eventType, Func<UniTask> action);
        void RemoveDialogueEvent(LocalizationKey key, DialogueEvent eventType, Func<UniTask> action);
        
        UniTask AwaitDialogueEvents(LocalizationKey key, DialogueEvent eventType, CancellationToken cancellationToken);

        void RegisterPrinter(IDialoguePrinter printer);
        void UnregisterPrinter(IDialoguePrinter printer);

        UniTask PrintElementAsync(LocalizationKey dialogue, LocalizationKey key, CancellationToken cancellationToken);
        void CancelCurrentElementPrinting(DialogueCancelMode mode);
        void ClearAllPrinters();
    }
    
}