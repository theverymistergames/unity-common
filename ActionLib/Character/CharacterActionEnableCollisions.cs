using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Character.Phys;
using MisterGames.Common.Attributes;
using UnityEngine;

namespace MisterGames.ActionLib.Character {
    
    [Serializable]
    public sealed class CharacterActionEnableCollisions : IActorAction {

        public BlockSourceMode blockSourceMode;
        [VisibleIf(nameof(blockSourceMode), 2)]
        public GameObject blockSource;
        public bool isEnabled;

        public enum BlockSourceMode {
            ThisAction,
            Actor,
            Explicit,
        }
        
        public UniTask Apply(IActor context, CancellationToken cancellationToken = default) {
            object source = blockSourceMode switch {
                BlockSourceMode.ThisAction => this,
                BlockSourceMode.Actor => context,
                BlockSourceMode.Explicit => blockSource,
                _ => throw new ArgumentOutOfRangeException()
            };
            
            context.GetComponent<CharacterCollisionPipeline>().Block(source, blocked: !isEnabled, cancellationToken);
            return default;
        }
    }
    
}
