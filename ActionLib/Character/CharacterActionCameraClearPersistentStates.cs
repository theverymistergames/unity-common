using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Character.View;
using MisterGames.Common.Data;
using MisterGames.Tick.Core;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MisterGames.ActionLib.Character {

    [Serializable]
    public sealed class CharacterActionCameraClearPersistentStates : IActorAction {
        
        [Min(0f)] public float duration;

        public UniTask Apply(IActor actor, CancellationToken cancellationToken = default) {
            return actor.GetComponent<CharacterViewPipeline>().CameraContainer.ClearPersistentStates(duration);
        }
    }

}
