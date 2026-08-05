using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace MisterGames.Common.Tick {
    
    public interface ITimescaleSystem {

        float GetTimescale();
        float GetTimescale(byte priority);
        float GetTimescale(TimescalePriority priority);
        
        void SetTimescale(object source, byte priority, float timescale);
        void SetTimescale(object source, TimescalePriority priority, float timescale);
        void RemoveTimescale(object source);
        
        UniTask ChangeTimescale(
            object source,
            byte priority,
            float timescale,
            float duration,
            bool removeOnFinish = false,
            AnimationCurve curve = null,
            CancellationToken cancellationToken = default
        );
        
        UniTask ChangeTimescale(
            object source,
            TimescalePriority priority,
            float timescale,
            float duration,
            bool removeOnFinish = false,
            AnimationCurve curve = null,
            CancellationToken cancellationToken = default
        );
    }
    
}