using UnityEngine;

namespace MisterGames.Common.Pooling {

    public sealed class ParentedGameObjectFactory : IPoolFactory<GameObject> {

        private readonly Transform _parent;

        public ParentedGameObjectFactory(Transform parent) {
            _parent = parent;
        }

        GameObject IPoolFactory<GameObject>.CreatePoolElement(GameObject sample) {
            var gameObject = Object.Instantiate(sample, Vector3.zero, Quaternion.identity, _parent);
            gameObject.name = sample.name;
            return gameObject;
        }

        void IPoolFactory<GameObject>.DestroyPoolElement(GameObject element) {
            Object.Destroy(element);
        }

        void IPoolFactory<GameObject>.ActivatePoolElement(GameObject element) {
            element.SetActive(true);
        }

        void IPoolFactory<GameObject>.DeactivatePoolElement(GameObject element) {
            element.SetActive(false);
        }
    }

}
