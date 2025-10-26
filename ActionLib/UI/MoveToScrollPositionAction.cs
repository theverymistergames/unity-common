using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.UI.Components;
using UnityEngine;

namespace MisterGames.ActionLib.UI {
    
    [Serializable]
    public sealed class MoveToScrollPositionAction : IActorAction {

        public ScrollRectHelper scrollHelper;
        [Range(0f, 1f)] public float x;
        [Range(0f, 1f)] public float y;
        [Min(0f)] public float duration = 0.5f;
        
        public UniTask Apply(IActor context, CancellationToken cancellationToken = default) {
            scrollHelper.MoveToPosition(new Vector2(x, y), duration);
            return UniTask.CompletedTask;
        }
    }
    
}