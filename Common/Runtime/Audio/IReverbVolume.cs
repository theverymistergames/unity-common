using MisterGames.Common.Volumes;
using UnityEngine;

namespace MisterGames.Common.Audio {

    public interface IReverbVolume {
        
        int Id { get; }
        int Priority { get; }
        float Level { get; }
        float Weight { get; set; }
        
        IReverbSettings GetReverbSettings();
        WeightData GetWeight(Vector3 position);
    }
    
}