namespace MisterGames.Common.Save {

    public interface ISaveable {

        void OnLoadData(ISaveSystem saveSystem);

        void OnSaveData(ISaveSystem saveSystem);
    }
    
}