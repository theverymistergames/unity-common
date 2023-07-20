using System;
using System.Collections.Generic;
using UnityEngine;

namespace MisterGames.Common.Dependencies {

    public sealed class RuntimeDependencyResolver : MonoBehaviour, IDependencyResolver, IDependencySetter {

        private readonly Dictionary<Type, object> _typeOverrides = new Dictionary<Type, object>();

#if UNITY_EDITOR
        internal readonly HashSet<Type> overridenTypes = new HashSet<Type>();
#endif

        public void SetValue<T>(T value) where T : class {
            _typeOverrides[typeof(T)] = value;
        }

        public T Resolve<T>() where T : class {
            if (_typeOverrides.TryGetValue(typeof(T), out object v)) return v as T;
            return default;
        }
    }

}
