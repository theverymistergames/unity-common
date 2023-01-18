using System;
using MisterGames.Blueprints.Core2;
using UnityEngine;

namespace MisterGames.BlueprintLib.Core2 {

    [Serializable]
    [BlueprintNodeMeta(Name = "Core2.Log", Category = "Core2.Test", Color = BlueprintColors.Node.Actions)]
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
        };

        public void OnEnterPort(int port) {
            if (port != 0) return;

            string text = $"<color=#{ColorUtility.ToHtmlStringRGB(_color)}>{ReadPort(1, _defaultText)}</color>";

            switch (_level) {
                case Level.Log:
                    Debug.Log(text);
                    break;
                case Level.Warning:
                    Debug.LogWarning(text);
                    break;
                case Level.Assertion:
                    Debug.LogAssertion(text);
                    break;
                case Level.Error:
                    Debug.LogError(text);
                    break;
            }
        }
    }

}
