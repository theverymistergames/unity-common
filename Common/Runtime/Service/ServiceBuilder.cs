namespace MisterGames.Common.Service {
    
    public readonly struct ServiceBuilder<T> {
        
        private readonly IServiceStorage _storage;
        private readonly T _service;
        private readonly int? _id;

        internal ServiceBuilder(IServiceStorage storage, T service, int? id = null) {
            _storage = storage;
            _service = service;
            _id = id;
        }

        public ServiceBuilder<T> AddType<S>() where S : class {
            if (_id is { } id) _storage.Register(_service as S, id);
            else _storage.RegisterGlobal(_service as S);
            
            return this;
        }
    }
    
}