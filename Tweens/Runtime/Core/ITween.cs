using System.Threading;
using Cysharp.Threading.Tasks;

namespace MisterGames.Tweens {

    /// <summary>
    /// Tween is a continuous behaviour with defined duration. Tweens can be played by <see cref="TweenPlayer{TContext,TTween}"/>,
    /// which uses progress [0f, 1f] and speed values to control tween playback and report progress.
    /// </summary>
    public interface ITween {

        /// <summary>
        /// Total duration of the tween.
        /// Can be created before each play in <see cref="CreateNextDuration"/> method.
        /// </summary>
        float Duration { get; }

        /// <summary>
        /// This method is called by <see cref="TweenPlayer{TContext,TTween}"/> to determine tween play duration before each launch.
        /// The result should be stored in <see cref="Duration"/> property.
        /// </summary>
        void CreateNextDuration();
    }
    
    /// <summary>
    /// Tween is a continuous behaviour with defined duration. Tweens can be played by <see cref="TweenPlayer{TContext,TTween}"/>,
    /// which uses progress [0f, 1f] and speed values to control tween playback and report progress.
    /// </summary>
    public interface ITween<in TContext> : ITween {

        /// <summary>
        /// Tween behaviour method.
        /// Should start play at startProgress [0f, 1f] with dt * speed time increment each frame.
        /// The target progress value is calculated from speed, if speed > 0, then target progress is 1, otherwise 0.
        /// </summary>
        /// <param name="context">Tween context object</param>
        /// <param name="duration">Total duration of current play call</param>
        /// <param name="startProgress">Start progress in range [0f, 1f]</param>
        /// <param name="speed">Frame delta time multiplier</param>
        /// <param name="cancellationToken">Token to cancel tween behaviour</param>
        /// <returns>UniTask with tween play behaviour</returns>
        UniTask Play(TContext context, float duration, float startProgress, float speed, CancellationToken cancellationToken = default);
    }

}
