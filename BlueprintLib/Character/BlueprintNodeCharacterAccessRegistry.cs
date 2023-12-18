using System;
using MisterGames.Blueprints;
using MisterGames.Character.Core;

namespace MisterGames.BlueprintLib {

    [Serializable]
    [BlueprintNode(Name = "Character Access Registry", Category = "Character", Color = BlueprintColors.Node.Actions)]
    public sealed class BlueprintNodeCharacterAccessRegistry :
        IBlueprintNode,
        IBlueprintEnter,
        IBlueprintOutput<CharacterAccess>,
        IBlueprintOutput<bool>
    {
        private IBlueprint _blueprint;
        private NodeToken _token;

        public void CreatePorts(IBlueprintMeta meta, NodeId id) {
            meta.AddPort(id, Port.Enter("Subscribe"));
            meta.AddPort(id, Port.Enter("Unsubscribe"));
            meta.AddPort(id, Port.Exit("On Registered"));
            meta.AddPort(id, Port.Exit("On Unregistered"));
            meta.AddPort(id, Port.Output<CharacterAccess>());
            meta.AddPort(id, Port.Output<bool>("Is Registered"));
        }

        public void OnInitialize(IBlueprint blueprint, NodeToken token, NodeId root) {
            _blueprint = blueprint;
            _token = token;
        }

        public void OnDeInitialize(IBlueprint blueprint, NodeToken token, NodeId root) {
            CharacterAccessRegistry.Instance.OnRegistered -= OnRegistered;
            CharacterAccessRegistry.Instance.OnUnregistered -= OnUnregistered;

            _blueprint = null;
        }

        public void OnEnterPort(IBlueprint blueprint, NodeToken token, int port) {
            if (port == 0) {
                CharacterAccessRegistry.Instance.OnRegistered -= OnRegistered;
                CharacterAccessRegistry.Instance.OnUnregistered -= OnUnregistered;

                CharacterAccessRegistry.Instance.OnRegistered += OnRegistered;
                CharacterAccessRegistry.Instance.OnUnregistered += OnUnregistered;

                return;
            }

            if (port == 1) {
                CharacterAccessRegistry.Instance.OnRegistered -= OnRegistered;
                CharacterAccessRegistry.Instance.OnUnregistered -= OnUnregistered;

                return;
            }
        }

        CharacterAccess IBlueprintOutput<CharacterAccess>.GetPortValue(IBlueprint blueprint, NodeToken token, int port) {
            return port == 4 ? CharacterAccessRegistry.Instance.GetCharacterAccess() : default;
        }

        bool IBlueprintOutput<bool>.GetPortValue(IBlueprint blueprint, NodeToken token, int port) {
            return port == 5 ? CharacterAccessRegistry.Instance.GetCharacterAccess() != null : default;
        }

        private void OnRegistered(CharacterAccess characterAccess) {
            _blueprint.Call(_token, 2);
        }

        private void OnUnregistered(CharacterAccess characterAccess) {
            _blueprint.Call(_token, 3);
        }
    }

}
