using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Character.Capsule;

namespace MisterGames.ActionLib.Character {
    
    [Serializable]
    public sealed class CharacterActionEnablePoseGraph : IActorAction {

        public bool isEnabled;

        public UniTask Apply(IActor context, CancellationToken cancellationToken = default) {
            context.GetComponent<CharacterPoseGraphPipeline>().enabled = isEnabled;
            return default;
        }
    }
    
}
