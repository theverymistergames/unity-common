﻿using System;
using MisterGames.Blueprints;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [Serializable]
    public class BlueprintSourceLog :
        BlueprintSource<BlueprintNodeLog>,
        BlueprintSources.IEnter<BlueprintNodeLog>,
        BlueprintSources.ICloneable {}

    [Serializable]
    [BlueprintNode(Name = "Log", Category = "Debug", Color = BlueprintColors.Node.Debug)]
    public struct BlueprintNodeLog : IBlueprintNode, IBlueprintEnter {

        [SerializeField] private Level _level;
        [SerializeField] private Color _color;
        [SerializeField] private string _defaultText;

        private enum Level {
            Log,
            Warning,
            Assertion,
            Error
        }

        public void OnSetDefaults(IBlueprintMeta meta, NodeId id) {
            _level = Level.Log;
            _color = Color.white;
        }

        public void CreatePorts(IBlueprintMeta meta, NodeId id) {
            meta.AddPort(id, Port.Enter());
            meta.AddPort(id, Port.Input<string>("Text"));
            meta.AddPort(id, Port.Exit());
        }

        public void OnEnterPort(IBlueprint blueprint, NodeToken token, int port) {
            if (port != 0) return;

            string text = blueprint.Read(token, 1, _defaultText);
            string formatText = $"<color=#{ColorUtility.ToHtmlStringRGB(_color)}>{text}</color>";

            switch (_level) {
                case Level.Log:
                    Debug.Log(formatText);
                    break;
                case Level.Warning:
                    Debug.LogWarning(formatText);
                    break;
                case Level.Assertion:
                    Debug.LogAssertion(formatText);
                    break;
                case Level.Error:
                    Debug.LogError(formatText);
                    break;
            }

            blueprint.Call(token, 2);
        }
    }

}
