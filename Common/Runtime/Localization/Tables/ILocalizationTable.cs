namespace MisterGames.Common.Localization {
    
    public interface ILocalizationTable {

        bool ContainsKey(int keyHash);
        bool TryGetKey(int keyHash, out string value);
        
        bool TryGetValue<T>(int keyHash, int localeHash, out T value);
    }
    
}