using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Common.Service;
using MisterGames.UI.Windows;
using UnityEngine;

namespace MisterGames.ActionLib.UI {
    
    [Serializable]
    public sealed class SetParentUiWindowStateAction : IActorAction {
        
        public GameObject relativeToGameObject;
        public UiWindowState state;
        
        public UniTask Apply(IActor context, CancellationToken cancellationToken = default) {
            if (Services.TryGet(out IUiWindowService service) && 
                service.FindClosestParentWindow(relativeToGameObject) is { } parentWindow) 
            {
                service.SetWindowState(parentWindow, state);
            }
            
            return UniTask.CompletedTask;
        }
    }
    
}