using System;
using System.Collections.Generic;
using UnityEngine;

namespace MisterGames.Common.Pooling {

    public sealed class ObjectPool<T> {

        private readonly IPoolFactory<T> _poolFactory;
        private readonly Queue<T> _elementQueue = new Queue<T>();
        private readonly T _sample;

        private float _ensureCapacityAt = 0f;
        private float _ensureCapacityCoeff = 1f;
        private int _targetCapacity = 1;

        public ObjectPool(T sample, IPoolFactory<T> poolFactory) {
            _sample = sample;
            _poolFactory = poolFactory;
        }

        public void Initialize(float ensureCapacityAt, float ensureCapacityCoeff, int initialCapacity = 0) {
            _ensureCapacityAt = ensureCapacityAt;
            _ensureCapacityCoeff = ensureCapacityCoeff;
            _targetCapacity = initialCapacity > 0 ? initialCapacity : 1;

            ValidateParameters();

            if (initialCapacity <= 0) return;
            EnsureCapacity(initialCapacity);
        }

        public void Clear() {
            for (int i = 0; i < _elementQueue.Count; i++) {
                var element = _elementQueue.Dequeue();
                _poolFactory.DestroyPoolElement(element);
            }
        }

        public T TakeActive() {
            EnsureCapacity();

            var element = _elementQueue.Dequeue();
            _poolFactory.ActivatePoolElement(element);

            return element;
        }

        public T TakeInactive() {
            EnsureCapacity();
            return _elementQueue.Dequeue();
        }

        public void Recycle(T element) {
            _poolFactory.DeactivatePoolElement(element);
            _elementQueue.Enqueue(element);
        }

        private void EnsureCapacity(int maxCapacity = -1) {
            int inPoolCount = _elementQueue.Count;
            int ensureAt = Mathf.FloorToInt(_targetCapacity * _ensureCapacityAt);

            if (inPoolCount > ensureAt) return;

            int neededCapacity = Mathf.CeilToInt(_targetCapacity * _ensureCapacityCoeff);
            _targetCapacity = maxCapacity < 0
                ? _targetCapacity
                : Math.Min(maxCapacity, neededCapacity);

            int instantiateCount = _targetCapacity - inPoolCount;
            for (int i = 0; i < instantiateCount; i++) {
                var newElement = _poolFactory.CreatePoolElement(_sample);
                Recycle(newElement);
            }
        }

        private void ValidateParameters() {
            if (_ensureCapacityAt < 0f || _ensureCapacityAt > 1f) {
                throw new ArgumentException($"{this} has invalid parameter [ensureCapacityAt = {_ensureCapacityAt}], value must be in range [0, 1]");
            }

            if (_ensureCapacityCoeff < 1f) {
                throw new ArgumentException($"{this} has invalid parameter [ensureCapacityCoeff = {_ensureCapacityCoeff}], value must be >= 1");
            }
        }

        public override string ToString() {
            return $"ObjectPool<{nameof(T)}>";
        }
    }

}
