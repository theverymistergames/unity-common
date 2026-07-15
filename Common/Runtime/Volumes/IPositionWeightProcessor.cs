namespace MisterGames.Common.Volumes {
    
    public interface IPositionWeightProcessor {

        void Initialize() { }
        void DeInitialize() { }
        
        float GetWeight();

        void OnValidate() { }
    }
    
}