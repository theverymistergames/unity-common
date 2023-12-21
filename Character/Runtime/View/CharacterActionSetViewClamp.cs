using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Character.Actions;
using MisterGames.Character.Core;
using MisterGames.Common.Data;

namespace MisterGames.Character.View {
    
    [Serializable]
    public sealed class CharacterActionSetViewClamp : ICharacterAction {

        public Optional<ViewAxisClamp> horizontal;
        public Optional<ViewAxisClamp> vertical;

        public UniTask Apply(ICharacterAccess characterAccess, CancellationToken cancellationToken = default) {
            var clamp = characterAccess
                .GetPipeline<ICharacterViewPipeline>()
                .GetProcessor<CharacterProcessorViewClamp>();

            if (horizontal.HasValue) clamp.horizontal = horizontal.Value;
            if (vertical.HasValue) clamp.vertical = vertical.Value;

            return default;
        }
    }
    
}
