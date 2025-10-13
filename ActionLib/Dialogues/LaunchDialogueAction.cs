using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Dialogues.Components;
using MisterGames.Dialogues.Core;

namespace MisterGames.ActionLib.Dialogues {
    
    [Serializable]
    public sealed class LaunchDialogueAction : IActorAction {

        public DialogueLauncher dialogueLauncher;
        public DialogueReference dialogueReference;
        public Mode mode;
        
        public enum Mode {
            Launch,
            Stop,
            Pause,
            Resume,
            TogglePause,
        }
        
        public UniTask Apply(IActor context, CancellationToken cancellationToken = default) {
            switch (mode) {
                case Mode.Launch:
                    return dialogueLauncher.LaunchDialogueAsync(dialogueReference.AssetGUID, cancellationToken);
                
                case Mode.Stop:
                    dialogueLauncher.StopDialogue();
                    return UniTask.CompletedTask;
                
                case Mode.Pause:
                    dialogueLauncher.PauseDialogue();
                    return UniTask.CompletedTask;
                
                case Mode.Resume:
                    dialogueLauncher.ResumeDialogue();
                    return UniTask.CompletedTask;
                
                case Mode.TogglePause:
                    if (dialogueLauncher.IsPaused) dialogueLauncher.ResumeDialogue();
                    else dialogueLauncher.PauseDialogue();
                    return UniTask.CompletedTask;
                
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
    
}