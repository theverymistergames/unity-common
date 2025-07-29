namespace MisterGames.Logic.Water {
    
    public interface IWaterZoneProxyCluster {
        
        int ClusterId { get; }
        int ProxyCount { get; }
        
        int GetProxyId(int index);
    }
    
}