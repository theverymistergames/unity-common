using System.Threading;
using Cysharp.Threading.Tasks;

namespace MisterGames.Tweens {
    
    public interface ITweenPlayer {
        
        event ProgressCallback OnProgressUpdate;
        
        float Duration { get; }
        float Timer { get; }
        float Progress { get; set; }
        float Speed { get; set; }
        YoyoMode Yoyo { get; set; }
        bool Loop { get; set; }
        bool InvertNextPlay { get; set; }

        UniTask<bool> Play<T>(
            T data,
            ProgressCallback<T> progressCallback,
            float progress = -1f,
            CancellationToken cancellationToken = default
        );

        UniTask<bool> Play(
            ProgressCallback progressCallback = null,
            float progress = -1f,
            CancellationToken cancellationToken = default
        );

        void Stop();
    }
    
}