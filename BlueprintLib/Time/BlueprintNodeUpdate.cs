using System;
using MisterGames.Blueprints;
using MisterGames.Common.Tick;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [Serializable]
    [BlueprintNode(Name = "Update", Category = "Time", Color = BlueprintColors.Node.Data)]
    public class BlueprintNodeUpdate : IBlueprintNode, IBlueprintEnter, IBlueprintOutput<float>, IUpdate {

        [SerializeField] private PlayerLoopStage _stage = PlayerLoopStage.Update;

        private ITimeSource _timeSource;
        private IBlueprint _blueprint;
        private NodeToken _token;

        public void CreatePorts(IBlueprintMeta meta, NodeId id) {
            meta.AddPort(id, Port.Enter("Start"));
            meta.AddPort(id, Port.Enter("Stop"));
            meta.AddPort(id, Port.Exit("On Update"));
            meta.AddPort(id, Port.Output<float>("dt"));
        }

        public void OnInitialize(IBlueprint blueprint, NodeToken token, NodeId root) {
            _timeSource = TimeSources.Get(_stage);
        }

        public void OnDeInitialize(IBlueprint blueprint, NodeToken token, NodeId root) {
            _timeSource.Unsubscribe(this);
            _timeSource = null;
            _blueprint = null;
        }

        public void OnEnterPort(IBlueprint blueprint, NodeToken token, int port) {
            _blueprint = blueprint;
            _token = token;

            if (port == 0) {
                _timeSource.Subscribe(this);
                return;
            }

            if (port == 1) {
                _timeSource.Unsubscribe(this);
                return;
            }
        }

        public float GetPortValue(IBlueprint blueprint, NodeToken token, int port) => port switch {
            3 => _timeSource.DeltaTime,
            _ => default,
        };

        public void OnUpdate(float dt) {
            _blueprint?.Call(_token, 2);
        }
    }

}
