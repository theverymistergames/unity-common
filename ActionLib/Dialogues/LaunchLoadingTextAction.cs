using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Common.Attributes;
using MisterGames.Common.Labels;
using MisterGames.Logic.Loading;
using UnityEngine;

namespace MisterGames.ActionLib.Dialogues {
    
    [Serializable]
    public sealed class LaunchLoadingTextAction : IActorAction {

        public LabelValue<UnityEngine.Object> loadingTextLauncher;
        public LoadingTextPreset preset;
        [SerializeReference] [SubclassSelector] public IActorAction loadAction;
        
        public UniTask Apply(IActor context, CancellationToken cancellationToken = default) {
            var launcher = loadingTextLauncher.GetData() as LoadingTextLauncher;
            if (launcher == null) {
                Debug.LogError($"LaunchLoadingTextAction.Apply: f {UnityEngine.Time.frameCount}, cannot find loading text launcher by id {loadingTextLauncher}");
                return default;
            }
            
            var action = new Func<UniTask>(() => loadAction?.Apply(context, cancellationToken) ?? UniTask.CompletedTask);
            
            return launcher.PrintLoadingText(preset, action, cancellationToken);
        }
    }
    
}