using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Common.Service;
using MisterGames.UI.UiServices;
using UnityEngine;

namespace MisterGames.ActionLib.UI {
    
    [Serializable]
    public sealed class BlockCursorAction : IActorAction {

        public Transform source;
        public bool block;
        
        public UniTask Apply(IActor context, CancellationToken cancellationToken = default) {
            Services.Get<ICursorService>().BlockCursor(source, block);
            return UniTask.CompletedTask;
        }
    }
    
}