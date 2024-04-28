using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Common.Data;
using MisterGames.Common.Pooling;
using UnityEngine;

namespace MisterGames.ActionLib.Character {
    
    [Serializable]
    public sealed class CharacterActionSpawnPrefab : IActorAction {

        public GameObject prefab;
        public Vector3 positionOffset;
        public Vector3 rotationOffset;
        public bool useLocal;
        public Optional<float> lifeTime;

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

            if (lifeTime.HasValue) {
                WaitAndDestroy(instance, lifeTime.Value, cancellationToken).Forget();
            }
            
            return default;
        }

        private static async UniTask WaitAndDestroy(GameObject gameObject, float delay, CancellationToken cancellationToken) {
            await UniTask
                .Delay(TimeSpan.FromSeconds(delay), cancellationToken: cancellationToken)
                .SuppressCancellationThrow();

            if (cancellationToken.IsCancellationRequested || gameObject == null) return;
            
            PrefabPool.Instance.Recycle(gameObject);
        }
    }
    
}
