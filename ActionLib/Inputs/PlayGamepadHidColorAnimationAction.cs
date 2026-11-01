using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Common.Inputs;
using MisterGames.Common.Service;

namespace MisterGames.ActionLib.Inputs {
    
    [Serializable]
    public sealed class PlayGamepadHidColorAnimationAction : IActorAction {
        
        public GamepadHidColorAnimationAsset animation;
        public bool wait = true;

        public async UniTask Apply(IActor context, CancellationToken cancellationToken = default) {
            if (animation == null) return;
            
            var hidColors = Services.Get<IDeviceService>().GamepadHidColors;
            hidColors.PlayAnimation(this, animation);

            var task = WatchAnimation(hidColors, cancellationToken);
            
            if (wait) await task;
            else task.Forget();
        }

        private async UniTask WatchAnimation(IGamepadHidColors hidColors, CancellationToken cancellationToken) {
            while (!cancellationToken.IsCancellationRequested && hidColors.IsAnimationPlaying(this)) {
                await UniTask.Yield();
            }
            
            hidColors.StopAnimation(this);
        }
        
    }
    
}
