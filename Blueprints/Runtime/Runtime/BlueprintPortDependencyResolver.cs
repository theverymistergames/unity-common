using System;
using System.Collections.Generic;
using MisterGames.Blueprints;
using MisterGames.Common.Dependencies;

namespace MisterGames.Blueprints.Runtime {

    public sealed class BlueprintPortDependencyResolver : IDependencyResolver, IDependencyContainer, IDependencyBucket {

        public int Count => _dependencies.Count;
        public Type this[int index] => _dependencies[index];

        private readonly List<Type> _dependencies = new List<Type>();
        private IBlueprint _blueprint;
        private NodeToken _token;
        private int _portPointer;

        public void Setup(IBlueprint blueprint, NodeToken token, int portOffset = 0) {
            _blueprint = blueprint;
            _token = token;
            _portPointer = portOffset;
        }

        public T Resolve<T>() where T : class {
            return _blueprint.Read<T>(_token, _portPointer++);
        }

        public IDependencyBucket CreateBucket(object source) {
            return this;
        }

        public IDependencyBucket Add<T>() where T : class {
            _dependencies.Add(typeof(T));
            return this;
        }

        public void Reset() {
            _dependencies.Clear();
            _blueprint = null;
            _token = default;
            _portPointer = 0;
        }
    }
}
