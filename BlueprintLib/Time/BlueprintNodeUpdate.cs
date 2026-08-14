using System;
using MisterGames.Blueprints;
using MisterGames.Common.Tick;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [Serializable]
    [BlueprintNode(Name = "Update", Category = "Time", Color = BlueprintColors.Node.Data)]
    public class BlueprintNodeUpdate : IBlueprintNode, IBlueprintEnter, IBlueprintOutput<float>, IUpdate {

        private IBlueprint _blueprint;
        private NodeToken _token;

        public void CreatePorts(IBlueprintMeta meta, NodeId id) {
            meta.AddPort(id, Port.Enter("Start"));
            meta.AddPort(id, Port.Enter("Stop"));
            meta.AddPort(id, Port.Exit("On Update"));
            meta.AddPort(id, Port.Output<float>("dt"));
        }

        public void OnInitialize(IBlueprint blueprint, NodeToken token, NodeId root) {
        }

        public void OnDeInitialize(IBlueprint blueprint, NodeToken token, NodeId root) {
            PlayerLoopStage.Update.Unsubscribe(this);
            _blueprint = null;
        }

        public void OnEnterPort(IBlueprint blueprint, NodeToken token, int port) {
            _blueprint = blueprint;
            _token = token;

            if (port == 0) {
                PlayerLoopStage.Update.Subscribe(this);
                return;
            }

            if (port == 1) {
                PlayerLoopStage.Update.Unsubscribe(this);
                return;
            }
        }

        public float GetPortValue(IBlueprint blueprint, NodeToken token, int port) => port switch {
            3 => TimeSources.deltaTime,
            _ => 0,
        };

        public void OnUpdate(float dt) {
            _blueprint?.Call(_token, 2);
        }
    }

}
