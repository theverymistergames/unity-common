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

        public void CreatePorts(IBlueprintMeta meta, NodeId id) {
            meta.AddPort(id, Port.Enter("Apply"));
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
        }

        public void OnEnterPort(IBlueprint blueprint, NodeToken token, int port) {
            if (port != 0) return;

            var actor = blueprint.Read<IActor>(token, 2);
            var action = blueprint.Read<IActorAction>(token, 3, _action);

            Apply(blueprint, token, actor, action, _terminateCts.Token).Forget();
        }

        private async UniTaskVoid Apply(
            IBlueprint blueprint,
            NodeToken token,
            IActor actor,
            IActorAction action,
            CancellationToken cancellationToken
        ) {
            await action.Apply(actor, cancellationToken);
            if (!cancellationToken.IsCancellationRequested) blueprint.Call(token, 1);
        }
    }

}
