using MisterGames.Common.Volumes;
using UnityEngine;

namespace MisterGames.Common.Audio {

    public interface IReverbVolume {
        
        int Id { get; }
        int Priority { get; }
        float Level { get; }
        
        IReverbSettings GetReverbSettings();
        WeightData GetWeight(Vector3 position);
    }
    
}