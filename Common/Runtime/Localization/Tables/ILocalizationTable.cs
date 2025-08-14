namespace MisterGames.Common.Localization {
    
    public interface ILocalizationTable {

        bool ContainsKey(int keyHash);

        bool TryGetValue(int keyHash, int localeHash, out string value);
    }
    
}