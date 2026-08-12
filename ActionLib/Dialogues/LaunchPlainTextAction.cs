using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Common.Attributes;
using MisterGames.Common.Labels;
using MisterGames.Logic.Loading;
using MisterGames.Logic.UI;
using UnityEngine;

namespace MisterGames.ActionLib.Dialogues {
    
    [Serializable]
    public sealed class LaunchPlainTextAction : IActorAction {

        public LabelValue<UnityEngine.Object> plainTextLauncher;
        public PlainTextPreset preset;
        public PlainTextLauncher.PrintOptions printOptions;
        
        public UniTask Apply(IActor context, CancellationToken cancellationToken = default) {
            var launcher = plainTextLauncher.GetData() as PlainTextLauncher;
            if (launcher == null) {
                Debug.LogError($"LaunchPlainTextAction.Apply: f {UnityEngine.Time.frameCount}, cannot find plain text launcher by id {plainTextLauncher}");
                return default;
            }
            
            return launcher.PrintPlainText(preset, printOptions, cancellationToken);
        }
    }
    
}