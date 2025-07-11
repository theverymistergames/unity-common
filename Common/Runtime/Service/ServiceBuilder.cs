using System;

namespace MisterGames.Common.Service {
    
    public readonly struct ServiceBuilder {
        
        private readonly IServiceStorage _storage;
        private readonly object _service;
        private readonly int? _id;

        internal ServiceBuilder(IServiceStorage storage, object service, int? id = null) {
            _storage = storage;
            _service = service;
            _id = id;
        }

        public ServiceBuilder AddType(Type type) {
            return _id is { } id 
                ? _storage.Register(_service, type, id) 
                : _storage.RegisterGlobal(_service, type);
        }
        
        public ServiceBuilder AddType<S>() where S : class {
            return AddType(typeof(S));
        }
    }
    
}