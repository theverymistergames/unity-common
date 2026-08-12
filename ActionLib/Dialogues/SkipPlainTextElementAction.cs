using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Common.Labels;
using MisterGames.Dialogues.Components;
using MisterGames.Logic.UI;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MisterGames.ActionLib.Dialogues {
    
    [Serializable]
    public sealed class SkipPlainTextElementAction : IActorAction {
        
        public LabelValue<Object> plainTextLauncherId;

        public UniTask Apply(IActor context, CancellationToken cancellationToken = default) {
            var plainTextLauncher = plainTextLauncherId.GetData() as PlainTextLauncher;
            if (plainTextLauncher == null) {
                Debug.LogError($"SkipPlainTextElementAction.Apply: f {UnityEngine.Time.frameCount}, cannot find plain text launcher by id {plainTextLauncherId}");
                return default;
            }
            
            plainTextLauncher.NotifySkip();
            return UniTask.CompletedTask;
        }
    }
    
}