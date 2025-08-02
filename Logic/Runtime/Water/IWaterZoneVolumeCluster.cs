namespace MisterGames.Logic.Water {
    
    public interface IWaterZoneVolumeCluster {
        
        int ClusterId { get; }
        int VolumeCount { get; }
        
        int GetVolumeId(int index);
    }
    
}