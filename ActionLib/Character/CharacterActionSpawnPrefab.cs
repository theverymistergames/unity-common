using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Common.Pooling;
using UnityEngine;

namespace MisterGames.ActionLib.Character {
    
    [Serializable]
    public sealed class CharacterActionSpawnPrefab : IActorAction {

        public GameObject prefab;
        public Vector3 positionOffset;
        public Vector3 rotationOffset;
        public bool useLocal;

        public UniTask Apply(IActor context, CancellationToken cancellationToken = default) {
            var pos = context.Transform.position;
            var rot = context.Transform.rotation;

            var instance = PrefabPool.Instance.TakeActive(prefab);

            if (useLocal) {
                instance.transform.SetPositionAndRotation(pos + rot * positionOffset, rot * Quaternion.Euler(rotationOffset));    
            }
            else {
                instance.transform.SetPositionAndRotation(pos + positionOffset, rot * Quaternion.Euler(rotationOffset));    
            }
            
            return default;
        }
    }
    
}
