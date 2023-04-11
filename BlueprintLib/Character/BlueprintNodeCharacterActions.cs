using System;
using MisterGames.Blueprints;
using MisterGames.Character.Core2.Access;
using MisterGames.Character.Core2.Actions;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [Serializable]
    [BlueprintNodeMeta(Name = "Character Actions", Category = "Character", Color = BlueprintColors.Node.Actions)]
    public sealed class BlueprintNodeCharacterActions : BlueprintNode, IBlueprintEnter {

        [SerializeField] private CharacterChangeSet[] _applyActions;
        [SerializeField] private CharacterChangeSet[] _releaseActions;

        private ICharacterAccess _characterAccess;

        public override Port[] CreatePorts() => new[] {
            Port.Enter("Apply"),
            Port.Enter("Release"),
            Port.Input<CharacterAccess>(),
            Port.Input<CharacterChangeSet>("Apply Set").Capacity(PortCapacity.Multiple),
            Port.Input<CharacterChangeSet>("Release Set").Capacity(PortCapacity.Multiple),
            Port.Exit("On Apply"),
            Port.Exit("On Release"),
        };

        public void OnEnterPort(int port) {
            switch (port) {
                case 0: {
                    _characterAccess ??= Ports[2].Get<CharacterAccess>();

                    for (int i = 0; i < _applyActions.Length; i++) {
                        _applyActions[i].Apply(this, _characterAccess);
                    }

                    var links = Ports[3].links;
                    for (int i = 0; i < links.Count; i++) {
                        if (links[i].Get<CharacterChangeSet>() is { } set) set.Apply(this, _characterAccess);
                    }

                    Ports[5].Call();
                    break;
                }

                case 1: {
                    _characterAccess ??= Ports[2].Get<CharacterAccess>();

                    for (int i = 0; i < _releaseActions.Length; i++) {
                        _releaseActions[i].Apply(this, _characterAccess);
                    }

                    var links = Ports[4].links;
                    for (int i = 0; i < links.Count; i++) {
                        if (links[i].Get<CharacterChangeSet>() is { } set) set.Apply(this, _characterAccess);
                    }

                    Ports[6].Call();
                    break;
                }
            }
        }
    }

}
