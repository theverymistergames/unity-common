using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors.Actions;
using MisterGames.Common.Attributes;
using MisterGames.Scenes.Core;
using MisterGames.Scenes.Utils;
using UnityEngine;

namespace MisterGames.ActionLib.Scenes {
    
    [Serializable]
    public sealed class SceneLoaderActionConverter : ISceneLoaderAction {
    
#if UNITY_EDITOR
        [SerializeField] private bool _bypassIfPlaymodeStartSceneOverriden = true;  
#endif
        [SerializeReference] [SubclassSelector] public IActorAction action;
        
        public UniTask Apply(CancellationToken cancellationToken) {
            bool applyAction = action != null;
            
#if UNITY_EDITOR
            applyAction &= !_bypassIfPlaymodeStartSceneOverriden || !PlaymodeStartScenesUtils.IsPlaymodeStartScenesOverrideEnabled(out _);
#endif
            
            return applyAction ? action.Apply(null, cancellationToken) : UniTask.CompletedTask;
        }
    }
    
}