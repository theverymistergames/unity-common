using MisterGames.Common.Save.Storages;
using MisterGames.Common.Save.Tables;

namespace MisterGames.Common.Save {
    
    public struct SaveBuilder {
        
        private readonly ISaveStorage<SaveKey> _storage;
        private readonly string _key;
        private int _index;

        public SaveBuilder(ISaveStorage<SaveKey> storage, string key, int index = 0) {
            _storage = storage;
            _key = key;
            _index = index;
        }

        public SaveBuilder Pop<T>(out T data) {
            data = _storage?.GetTable<T>() is { } table && table.TryGetData(new SaveKey(_key, _index++), out T d)
                ? d
                : default;
            return this;
        }
        
        public SaveBuilder Pop<T>(T def, out T data) {
            data = _storage?.GetTable<T>() is { } table && table.TryGetData(new SaveKey(_key, _index++), out T d)
                ? d
                : def;
            return this;
        }

        public SaveBuilder Push<T>(T data) {
            _storage?.GetOrCreateTable<T>().SetData(new SaveKey(_key, _index++), data);
            return this;
        }
    }
    
}