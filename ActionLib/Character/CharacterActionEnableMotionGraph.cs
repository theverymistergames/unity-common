using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Character.Motion;
using UnityEngine;

namespace MisterGames.ActionLib.Character {
    
    [Serializable]
    public sealed class CharacterActionEnableMotionGraph : IActorAction {

        public Transform source;
        public bool isEnabled;

        public UniTask Apply(IActor context, CancellationToken cancellationToken = default) {
            object source = this.source == null ? this : this.source;
            
            context.GetComponent<CharacterMotionGraphPipeline>()
                .SetBlock(source, blocked: !isEnabled, cancellationToken);
            
            return default;
        }
    }
    
}
