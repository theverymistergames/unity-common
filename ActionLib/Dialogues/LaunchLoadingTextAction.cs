using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Common.Labels;
using MisterGames.Dialogues.Components;
using MisterGames.Dialogues.Core;
using MisterGames.Logic.Loading;
using UnityEngine;

namespace MisterGames.ActionLib.Dialogues {
    
    [Serializable]
    public sealed class LaunchLoadingTextAction : IActorAction {

        public LabelValue<UnityEngine.Object> loadingTextLauncher;
        public LoadingTextPreset preset;
        public bool wait;
        
        public UniTask Apply(IActor context, CancellationToken cancellationToken = default) {
            var launcher = loadingTextLauncher.GetData() as LoadingTextLauncher;
            if (launcher == null) {
                Debug.LogError($"LaunchLoadingTextAction.Apply: f {UnityEngine.Time.frameCount}, cannot find loading text launcher by id {loadingTextLauncher}");
                return default;
            }
            if (wait) {
                return launcher.PrintLoadingText(preset, cancellationToken);
            }
            
            launcher.PrintLoadingText(preset, cancellationToken).Forget();
            return UniTask.CompletedTask;
        }
    }
    
}