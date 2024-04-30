namespace MisterGames.Common.Save {
    
    public struct SaveBuilder {
        
        private readonly ISaveSystem _saveSystem;
        private readonly int _id;
        private int _index;

        internal SaveBuilder(ISaveSystem saveSystem, int id, int index = 0) {
            _saveSystem = saveSystem;
            _id = id;
            _index = index;
        }

        public SaveBuilder Pop<T>(out T data) {
            data = _saveSystem.Get<T>(_id, _index++);
            return this;
        }
        
        public SaveBuilder Pop<T>(T def, out T data) {
            if (!_saveSystem.TryGet(_id, _index++, out data)) data = def;
            return this;
        }

        public SaveBuilder Push<T>(T data) {
            _saveSystem.Set(_id, _index++, data);
            return this;
        }
    }
    
}