using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using UnityEngine;
using UnityEngine.UI;

namespace MisterGames.ActionLib.UI {
    
    [Serializable]
    public sealed class SetSpriteIntoSpriteRendererAction : IActorAction {

        public SpriteRenderer spriteRenderer;
        public Sprite sprite;
        
        public UniTask Apply(IActor context, CancellationToken cancellationToken = default) {
            spriteRenderer.sprite = sprite;
            return default;
        }
    }
    
}