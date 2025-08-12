namespace MisterGames.Common.Save {
    
    public struct SaveBuilder {
        
        private readonly ISaveSystem _saveSystem;
        private readonly SaveStorage _storage;
        private readonly string _id;
        private int _index;

        internal SaveBuilder(ISaveSystem saveSystem, SaveStorage storage, string id, int index = 0) {
            _saveSystem = saveSystem;
            _storage = storage;
            _id = id;
            _index = index;
        }

        public SaveBuilder Pop<T>(out T data) {
            data = _saveSystem.Get<T>(_storage, _id, _index++);
            return this;
        }
        
        public SaveBuilder Pop<T>(T def, out T data) {
            if (!_saveSystem.TryGet(_storage, _id, _index++, out data)) data = def;
            return this;
        }

        public SaveBuilder Push<T>(T data) {
            _saveSystem.Set(_storage, _id, _index++, data);
            return this;
        }
    }
    
}