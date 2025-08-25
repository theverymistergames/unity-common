using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Input.Bindings;
using UnityEngine;

namespace MisterGames.ActionLib.Flow {
    
    [Serializable]
    public sealed class HotkeyAction : IActorAction {

        public KeyBinding key;
        public ShortcutModifiers modifiers;
        
        [Min(0f)] public float delay;
        
        public async UniTask Apply(IActor context, CancellationToken cancellationToken = default) {
            while (!cancellationToken.IsCancellationRequested && !IsActive()) {
                await UniTask.Yield();
            }

            if (cancellationToken.IsCancellationRequested) return;
            
            await UniTask.Delay(TimeSpan.FromSeconds(delay), cancellationToken: cancellationToken)
                .SuppressCancellationThrow();
        }

        private bool IsActive() {
            return key.IsPressed() && modifiers.ArePressed();
        }
    }
    
}