namespace MisterGames.Common.Save {
    
    public struct SaveBuilder {
        
        private readonly ISaveSystem _saveSystem;
        private readonly string _storageId;
        private readonly string _dataId;
        private int _index;

        public SaveBuilder(ISaveSystem saveSystem, string storageId, string dataId, int index = 0) {
            _saveSystem = saveSystem;
            _storageId = storageId;
            _dataId = dataId;
            _index = index;
        }

        public SaveBuilder Pop<T>(out T data) {
            data = _saveSystem.Get<T>(_storageId, _dataId, _index++);
            return this;
        }
        
        public SaveBuilder Pop<T>(T def, out T data) {
            if (!_saveSystem.TryGet(_storageId, _dataId, _index++, out data)) data = def;
            return this;
        }

        public SaveBuilder Push<T>(T data) {
            _saveSystem.Set(_storageId, _dataId, _index++, data);
            return this;
        }
    }
    
}