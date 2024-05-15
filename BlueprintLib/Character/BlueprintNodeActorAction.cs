using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Blueprints;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [Serializable]
    public class BlueprintSourceActorAction :
        BlueprintSource<BlueprintNodeActorAction>, 
        BlueprintSources.IEnter<BlueprintNodeActorAction>,
        BlueprintSources.ICloneable {}

    [Serializable]
    [BlueprintNode(Name = "Actor Action", Category = "Actors", Color = BlueprintColors.Node.Actions)]
    public struct BlueprintNodeActorAction : IBlueprintNode, IBlueprintEnter {

        [SerializeField] private ActorAction _action;

        private CancellationTokenSource _terminateCts;
        private CancellationTokenSource _actionCts;

        public void CreatePorts(IBlueprintMeta meta, NodeId id) {
            meta.AddPort(id, Port.Enter("Apply"));
            meta.AddPort(id, Port.Enter("Cancel"));
            meta.AddPort(id, Port.Exit("On Applied"));
            meta.AddPort(id, Port.Input<IActor>());
            meta.AddPort(id, Port.Input<IActorAction>());
        }

        public void OnInitialize(IBlueprint blueprint, NodeToken token, NodeId root) {
            _terminateCts?.Cancel();
            _terminateCts?.Dispose();
            _terminateCts = new CancellationTokenSource();
        }

        public void OnDeInitialize(IBlueprint blueprint, NodeToken token, NodeId root) {
            _terminateCts?.Cancel();
            _terminateCts?.Dispose();
            _terminateCts = null;
            
            _actionCts?.Cancel();
            _actionCts?.Dispose();
            _actionCts = null;
        }

        public void OnEnterPort(IBlueprint blueprint, NodeToken token, int port) {
            switch (port) {
                case 0: 
                    _actionCts ??= new CancellationTokenSource();
                    var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(_actionCts.Token, _terminateCts.Token);
                
                    var actor = blueprint.Read<IActor>(token, 3);
                    var action = blueprint.Read<IActorAction>(token, 4, _action);

                    Apply(blueprint, token, actor, action, linkedCts.Token).Forget();
                    break;
                
                case 1:
                    _actionCts?.Cancel();
                    _actionCts?.Dispose();
                    _actionCts = null;
                    break;
            }
        }

        private async UniTaskVoid Apply(
            IBlueprint blueprint,
            NodeToken token,
            IActor actor,
            IActorAction action,
            CancellationToken cancellationToken
        ) {
            if (action != null) await action.Apply(actor, cancellationToken);
            if (!cancellationToken.IsCancellationRequested) blueprint.Call(token, 2);
        }
    }

}
