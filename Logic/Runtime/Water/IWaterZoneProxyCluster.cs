namespace MisterGames.Logic.Water {
    
    public interface IWaterZoneProxyCluster {
        
        int VolumeId { get; }
        int ProxyCount { get; }
        
        int GetVolumeId(int index);
    }
    
}