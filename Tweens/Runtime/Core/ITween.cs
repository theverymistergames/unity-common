using System.Threading;
using Cysharp.Threading.Tasks;

namespace MisterGames.Tweens {

    /// <summary>
    /// Tween is a continuous behaviour with defined duration. Tweens can be played by <see cref="TweenPlayer"/>,
    /// which uses progress [0f, 1f] and speed values to control tween playback and report progress.
    /// </summary>
    public interface ITween {

        /// <summary>
        /// This method is called by <see cref="TweenPlayer"/> to determine tween play duration before its launch.
        /// The result is passed as "duration" float parameter into method <see cref="Play"/>.
        /// </summary>
        /// <returns>New duration for next play</returns>
        float CreateDuration();

        /// <summary>
        /// Tween behaviour method.
        /// Should start play at startProgress [0f, 1f] with dt * speed time increment each frame.
        /// The target progress value is calculated from speed, if speed > 0, then target progress is 1, otherwise 0.
        /// </summary>
        /// <param name="duration">Total duration of current play call</param>
        /// <param name="startProgress">Start progress in range [0f, 1f]</param>
        /// <param name="speed">Frame delta time multiplier</param>
        /// <param name="cancellationToken">Token to cancel tween behaviour</param>
        /// <returns>UniTask with tween play behaviour</returns>
        UniTask Play(float duration, float startProgress, float speed, CancellationToken cancellationToken = default);
    }

}
