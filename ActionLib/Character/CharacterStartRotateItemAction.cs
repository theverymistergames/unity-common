using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Character.View;
using UnityEngine;

namespace MisterGames.ActionLib.Character {
    
    [Serializable]
    public sealed class CharacterStartRotateItemAction : IActorAction {

        public Transform item;
        public Vector2 sensitivity = Vector2.one;
        public RotationPlane plane;
        public float smoothing;
        
        public UniTask Apply(IActor context, CancellationToken cancellationToken = default) {
            context.GetComponent<CharacterViewPipeline>().RotateObject(item, sensitivity, plane, smoothing);
            return default;
        }
    }
    
}