using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Character.Capsule;
using UnityEngine;

namespace MisterGames.ActionLib.Character {
    
    [Serializable]
    public sealed class CharacterActionEnablePoseGraph : IActorAction {

        public Transform source;
        public bool isEnabled;
        public bool forceStand;

        public UniTask Apply(IActor context, CancellationToken cancellationToken = default) {
            object source = this.source == null ? this : this.source;
            
            var pose = context.GetComponent<CharacterPoseGraphPipeline>();
                
            pose.SetBlock(source, blocked: !isEnabled, cancellationToken);
            if (forceStand) pose.ApplyStandPose();
            
            return default;
        }
    }
    
}
