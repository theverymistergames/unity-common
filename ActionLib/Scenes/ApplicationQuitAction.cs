using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MisterGames.ActionLib.Scenes {
    
    [Serializable]
    public sealed class ApplicationQuitAction : IActorAction {
        
        public UniTask Apply(IActor context, CancellationToken cancellationToken = default) {
#if UNITY_EDITOR
            EditorApplication.ExitPlaymode();
#else
            Application.Quit();
#endif
            return default;
        }
    }
    
}