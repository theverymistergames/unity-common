using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Common.Labels;
using MisterGames.Dialogues.Components;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MisterGames.ActionLib.Dialogues {
    
    [Serializable]
    public sealed class FinishDialogueElementAction : IActorAction {

        public LabelValue<Object> dialogueLauncherId;
        [Min(-1f)] public float symbolDelay = -1f;
        
        public UniTask Apply(IActor context, CancellationToken cancellationToken = default) {
            var dialogueLauncher = dialogueLauncherId.GetData() as DialogueLauncher;
            if (dialogueLauncher == null) {
                Debug.LogError($"FinishDialogueElementAction.Apply: f {UnityEngine.Time.frameCount}, cannot find dialogue launcher by id {dialogueLauncherId}");
                return default;
            }
            
            dialogueLauncher.FinishLastPrinting(symbolDelay);
            return UniTask.CompletedTask;
        }
    }
    
}