using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Input.Actions;

namespace MisterGames.ActionLib.Inputs {
    
    [Serializable]
    public sealed class AwaitAnyInputAction : IActorAction {

        public InputActionRef[] inputs;
        
        public async UniTask Apply(IActor context, CancellationToken cancellationToken = default) {
            bool pressed = false;

            while (!cancellationToken.IsCancellationRequested) {
                for (int i = 0; i < inputs.Length; i++) {
                    if (!inputs[i].Get().IsPressed()) continue;
                    pressed = true;
                    break;
                }
                if (pressed) break;
                await UniTask.Yield();
            }
        }
    }
    
}