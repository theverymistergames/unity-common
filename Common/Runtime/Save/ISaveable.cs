namespace MisterGames.Common.Save {

    public interface ISaveable {

        void OnLoadData(ISaveSystem saveSystem);

        void OnSaveData(ISaveSystem saveSystem);

        void OnAfterSaveData(ISaveSystem saveSystem) { }
        
        void OnAfterLoadData(ISaveSystem saveSystem) { }
    }
    
}