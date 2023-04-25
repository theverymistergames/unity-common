using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Character.Core;

namespace MisterGames.Character.Animations {

    public interface ICharacterAnimationPipeline : ICharacterPipeline {

        UniTask ApplyAnimation(
            object source,
            ICharacterAnimationPattern pattern,
            float duration,
            CancellationToken cancellationToken = default
        );
    }

}
