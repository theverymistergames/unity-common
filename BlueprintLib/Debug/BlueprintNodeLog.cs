using System;
using MisterGames.Blueprints;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [Serializable]
    [BlueprintNodeMeta(Name = "Log", Category = "Debug", Color = BlueprintColors.Node.Debug)]
    public sealed class BlueprintNodeLog : BlueprintNode, IBlueprintEnter {
        
        [SerializeField] private Level _level = Level.Log;
        [SerializeField] private Color _color = Color.white;
        [SerializeField] private string _defaultText = "";

        private enum Level {
            Log,
            Warning,
            Assertion,
            Error
        }

        public override Port[] CreatePorts() => new[] {
            Port.Enter(),
            Port.Input<string>("Text"),
            Port.Exit(),
        };

        public void OnEnterPort(int port) {
            if (port != 0) return;

            string text = ReadInputPort(1, _defaultText);
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

            CallExitPort(2);
        }
    }

}
