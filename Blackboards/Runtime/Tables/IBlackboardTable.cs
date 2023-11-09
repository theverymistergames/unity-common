namespace MisterGames.Blackboards.Tables {

    public interface IBlackboardTable {

        int Count { get; }

        T Get<T>(int hash);

        void Set<T>(int hash, T value);

        bool Contains(int hash);

        bool TryGetValue(int hash, out object value);

        void SetOrAddValue(int hash, object value);

        bool RemoveValue(int hash);

        string GetSerializedPropertyPath(int hash);
    }

}
