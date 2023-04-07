using System;
using MisterGames.Blueprints;
using MisterGames.Character.Core2;
using MisterGames.Character.Core2.Motion;
using MisterGames.Character.Core2.Processors;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [Serializable]
    [BlueprintNodeMeta(Name = "Apply Character Motion State Settings", Category = "Character", Color = BlueprintColors.Node.Actions)]
    public sealed class BlueprintNodeApplyCharacterMotionStateSettings : BlueprintNode, IBlueprintEnter {

        [SerializeField] private CharacterMotionStateSettings motionStateSettings;

        public override Port[] CreatePorts() => new[] {
            Port.Enter("Apply"),
            Port.Input<CharacterAccess>(),
        };

        public void OnEnterPort(int port) {
            if (port != 0) return;

            var characterAccess = Ports[1].Get<CharacterAccess>();
            var motionPipeline = characterAccess.MotionPipeline;

            if (motionPipeline.GetProcessor<CharacterProcessorVector2Multiplier>() is { } m) {
                m.multiplier = motionStateSettings.speed;
            }

            if (motionPipeline.GetProcessor<CharacterProcessorBackSideSpeedCorrection>() is { } c) {
                c.speedCorrectionBack = motionStateSettings.backCorrection;
                c.speedCorrectionSide = motionStateSettings.sideCorrection;
            }

            characterAccess.JumpPipeline.ForceMultiplier = motionStateSettings.jumpForceMultiplier;
        }
    }

}
