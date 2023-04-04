using System;
using MisterGames.Blueprints;
using MisterGames.Character.Configs;
using MisterGames.Character.Core2;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [Serializable]
    [BlueprintNodeMeta(Name = "Apply Character Motion Settings", Category = "Character", Color = BlueprintColors.Node.Actions)]
    public sealed class BlueprintNodeApplyCharacterMotionSettings : BlueprintNode, IBlueprintEnter {

        [SerializeField] private CharacterMotionSettings _motionSettings;

        public override Port[] CreatePorts() => new[] {
            Port.Enter("Apply"),
            Port.Input<CharacterAccess>(),
        };

        public void OnEnterPort(int port) {
            if (port != 0) return;

            var characterAccess = Ports[1].Get<CharacterAccess>();
            var motionPipeline = characterAccess.MotionPipeline;

            if (motionPipeline.GetProcessor<CharacterProcessorVector2Multiplier>() is { } m) {
                m.multiplier = _motionSettings.speed;
            }

            if (motionPipeline.GetProcessor<CharacterProcessorBackSideSpeedCorrection>() is { } c) {
                c.speedCorrectionBack = _motionSettings.backCorrection;
                c.speedCorrectionSide = _motionSettings.sideCorrection;
            }

            characterAccess.JumpProcessor.Force = _motionSettings.jumpForce;
        }
    }

}
