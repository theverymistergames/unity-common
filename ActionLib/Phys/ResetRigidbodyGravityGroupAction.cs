using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Common.Labels;
using MisterGames.Common.Service;
using MisterGames.Logic.Phys;

namespace MisterGames.ActionLib.Phys {
    
    [Serializable]
    public sealed class ResetRigidbodyGravityGroupAction : IActorAction {

        public LabelValue groupId;
        
        public UniTask Apply(IActor context, CancellationToken cancellationToken = default) {
            Services.Get<RigidbodyCustomGravityGroup>(groupId.GetValue())?.ResetAllMembers();
            return UniTask.CompletedTask;
        }
    }
    
}