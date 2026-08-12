using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Common.Labels;
using MisterGames.Logic.Loading;
using MisterGames.Logic.UI;
using Object = UnityEngine.Object;

namespace MisterGames.ActionLib.Dialogues {
    
    [Serializable]
    public sealed class ClearPlainTextAction : IActorAction {

        public LabelValue<Object> loadingTextLauncher;
        
        public UniTask Apply(IActor context, CancellationToken cancellationToken = default) {
            if (loadingTextLauncher.GetData() is PlainTextLauncher launcher) {
                launcher.ClearAllText();
            }

            return default;
        }
    }
    
}