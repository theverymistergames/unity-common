using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace MisterGames.Common.Tick {
    
    public interface ITimescaleSystem {
    
        void SetTimeScale(object source, int priority, float timeScale);
        void RemoveTimeScale(object source);
        
        UniTask ChangeTimeScale(
            object source,
            int priority,
            float timescale,
            float duration,
            bool removeOnFinish = false,
            AnimationCurve curve = null,
            CancellationToken cancellationToken = default
        );
    }
    
}