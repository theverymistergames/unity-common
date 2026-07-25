using MisterGames.Common.Data;

namespace MisterGames.Common.Localization {
    
    public interface ILocalizationTable {

        bool CanUnload();
        bool TryGetKey(int keyHash, out string value);
        bool TryGetValue<T>(int keyHash, int localeHash, out T value);
        bool TryGetDisposableValue<T>(int keyHash, int localeHash, out Disposable<T> disposableValue);
    }
    
}