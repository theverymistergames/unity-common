using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using UnityEngine;
using UnityEngine.UI;

namespace MisterGames.ActionLib.UI {
    
    [Serializable]
    public sealed class SetSpriteIntoImageAction : IActorAction {

        public Image image;
        public Sprite sprite;
        
        public UniTask Apply(IActor context, CancellationToken cancellationToken = default) {
            image.sprite = sprite;
            return default;
        }
    }
    
}