namespace MisterGames.Common.Editor.Search {

    public interface IObjectSearchFilter {

        void SetupRequest(AssetFileScanner.Request request);

        bool IsMatch(AssetFileScanner.ScannedFile file, AssetFileScanner.ScannedObject obj);
    }

}
