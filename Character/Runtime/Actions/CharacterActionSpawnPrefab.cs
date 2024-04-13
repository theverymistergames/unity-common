using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Character.Core;
using MisterGames.Character.Input;
using MisterGames.Common.Pooling;
using UnityEngine;

namespace MisterGames.Character.Actions {
    
    [Serializable]
    public sealed class CharacterActionSpawnPrefab : ICharacterAction {

        public GameObject prefab;
        public Vector3 positionOffset;
        public Vector3 rotationOffset;
        public bool useLocal;

        public UniTask Apply(ICharacterAccess characterAccess, CancellationToken cancellationToken = default) {
            var pos = characterAccess.BodyAdapter.Position;
            var rot = characterAccess.BodyAdapter.Rotation;

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
