using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Character.Actions;
using MisterGames.Character.Core;
using MisterGames.Common.Attributes;
using UnityEngine;

namespace MisterGames.Character.Animations {

    [Serializable]
    public sealed class CharacterActionApplyAnimation : ICharacterAsyncAction {

        [SerializeReference] [SubclassSelector] public ICharacterAnimationPattern pattern;
        [Min(0f)] public float duration;

        public UniTask ApplyAsync(object source, ICharacterAccess characterAccess, CancellationToken cancellationToken = default) {
            return characterAccess
                .GetPipeline<ICharacterAnimationPipeline>()
                .ApplyAnimation(source, pattern, duration, cancellationToken);
        }
    }

}
