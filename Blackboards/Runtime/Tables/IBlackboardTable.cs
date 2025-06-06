namespace MisterGames.Blackboards.Tables {

    public interface IBlackboardTable {

        T Get<T>(int hash);

        void Set<T>(int hash, T value);
        
        int Count { get; }
        
        bool Contains(int hash);

        bool TryGetValue(int hash, out object value);

        void SetOrAddValue(int hash, object value);

        bool RemoveValue(int hash);

        string GetSerializedPropertyPath(int hash);
    }

    public interface IBlackboardTable<T> : IBlackboardTable {
        
        T Get(int hash);

        void Set(int hash, T value);
    }
    
}
