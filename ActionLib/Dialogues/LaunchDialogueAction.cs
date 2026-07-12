using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Common.Labels;
using MisterGames.Dialogues.Components;
using MisterGames.Dialogues.Core;
using UnityEngine;

namespace MisterGames.ActionLib.Dialogues {
    
    [Serializable]
    public sealed class LaunchDialogueAction : IActorAction {

        public LabelValue<UnityEngine.Object> dialogueLauncherId;
        public DialogueReference dialogueReference;
        public Mode mode;
        public bool wait;
        
        public enum Mode {
            Launch,
            Stop,
            Pause,
            Resume,
            TogglePause,
        }
        
        public UniTask Apply(IActor context, CancellationToken cancellationToken = default) {
            var dialogueLauncher = dialogueLauncherId.GetData() as DialogueLauncher;
            if (dialogueLauncher == null) {
                Debug.LogError($"LaunchDialogueAction.Apply: f {UnityEngine.Time.frameCount}, cannot find dialogue launcher by id {dialogueLauncherId}");
                return default;
            }
            
            switch (mode) {
                case Mode.Launch:
                    if (wait) {
                        return dialogueLauncher.LaunchDialogueAsync(dialogueReference.AssetGUID, cancellationToken);
                    }
                    
                    dialogueLauncher.LaunchDialogueAsync(dialogueReference.AssetGUID, cancellationToken).Forget();
                    return UniTask.CompletedTask;
                
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