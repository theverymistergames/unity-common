using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Blueprints;
using MisterGames.Character.Actions;
using MisterGames.Character.Core;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [Serializable]
    public class BlueprintSourceCharacterAction :
        BlueprintSource<BlueprintNodeCharacterAction>,
        BlueprintSources.IEnter<BlueprintNodeCharacterAction>,
        BlueprintSources.ICloneable {}

    [Serializable]
    [BlueprintNode(Name = "Character Action", Category = "Character", Color = BlueprintColors.Node.Actions)]
    public struct BlueprintNodeCharacterAction : IBlueprintNode, IBlueprintEnter {

        [SerializeField] private CharacterActionAsset _action;

        private CancellationTokenSource _terminateCts;

        public void CreatePorts(IBlueprintMeta meta, NodeId id) {
            meta.AddPort(id, Port.Enter("Apply"));
            meta.AddPort(id, Port.Exit("On Applied"));
            meta.AddPort(id, Port.Input<ICharacterAccess>());
            meta.AddPort(id, Port.Input<ICharacterAction>());
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

            var characterAccess = blueprint.Read<ICharacterAccess>(token, 2);
            var action = blueprint.Read<ICharacterAction>(token, 3, _action);

            Apply(blueprint, token, characterAccess, action, _terminateCts.Token).Forget();
        }

        private async UniTaskVoid Apply(
            IBlueprint blueprint,
            NodeToken token,
            ICharacterAccess characterAccess,
            ICharacterAction action,
            CancellationToken cancellationToken
        ) {
            await action.Apply(characterAccess, cancellationToken);
            if (!cancellationToken.IsCancellationRequested) blueprint.Call(token, 1);
        }
    }

}
