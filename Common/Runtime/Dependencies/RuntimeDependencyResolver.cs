using System;
using System.Collections.Generic;
using UnityEngine;

namespace MisterGames.Common.Dependencies {

    public sealed class RuntimeDependencyResolver : MonoBehaviour, IDependencyOverride {

        private readonly Dictionary<Type, object> _dependenciesByType = new Dictionary<Type, object>();

#if UNITY_EDITOR
        internal readonly HashSet<Type> overridenTypes = new HashSet<Type>();
#endif

        public void SetDependenciesOfType<T>(T value) where T : class {
            _dependenciesByType[typeof(T)] = value;
        }

        public bool TryResolveDependencyOverride<T>(out T value) {
            if (_dependenciesByType.TryGetValue(typeof(T), out object v)) {
                value = v is T t ? t : default;
                return true;
            }

            value = default;
            return false;
        }
    }

}
