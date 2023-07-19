using System;
using System.Collections.Generic;
using UnityEngine;

namespace MisterGames.Common.Dependencies {

    public sealed class RuntimeDependencyResolver : MonoBehaviour, IDependencyOverride {

        private readonly Dictionary<Type, object> _typeOverrides = new Dictionary<Type, object>();

#if UNITY_EDITOR
        internal readonly HashSet<Type> overridenTypes = new HashSet<Type>();
#endif

        public void OverrideDependenciesOfType<T>(T value) where T : class {
            _typeOverrides[typeof(T)] = value;
        }

        public bool TryResolve<T>(out T value) where T : class {
            if (_typeOverrides.TryGetValue(typeof(T), out object v)) {
                value = v as T;
                return true;
            }

            value = default;
            return false;
        }
    }

}
