using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Common.Async;
using MisterGames.Input.Bindings;
using UnityEngine;

namespace MisterGames.ActionLib.Flow {
    
    [Serializable]
    public sealed class HotkeyAction : IActorAction {

        public KeyBinding key;
        public ShortcutModifiers modifiers;
        
        [Min(0f)] public float delay;
        public bool useUnscaledTime;

        public async UniTask Apply(IActor context, CancellationToken cancellationToken = default) {
            while (!cancellationToken.IsCancellationRequested && !IsActive()) {
                await UniTask.Yield();
            }

            if (cancellationToken.IsCancellationRequested) return;

            await AsyncExt.Delay(TimeSpan.FromSeconds(delay), ignoreTimeScale: useUnscaledTime, cancellationToken);
        }

        private bool IsActive() {
            return key.IsPressed() && modifiers.ArePressed();
        }
    }
    
}