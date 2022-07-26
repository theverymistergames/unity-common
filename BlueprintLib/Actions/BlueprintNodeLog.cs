using System.Collections.Generic;
using MisterGames.Blueprints;
using MisterGames.Blueprints.Core;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [BlueprintNode(Name = "Log", Category = "Actions", Color = BlueprintColors.Node.Actions)]
    public sealed class BlueprintNodeLog : BlueprintNode, IBlueprintEnter {
        
        [SerializeField] private Level _level = Level.Log;
        [SerializeField] private Color _color = Color.white;
        [SerializeField] private string _defaultText = "";
        
        protected override IReadOnlyList<Port> CreatePorts() => new List<Port> {
            Port.Enter(),
            Port.Input<string>("Text"),
            Port.Exit(),
        };

        void IBlueprintEnter.Enter(int port) {
            if (port != 0) return;

            string text = $"<color=#{ColorUtility.ToHtmlStringRGB(_color)}>{Read(1, _defaultText)}</color>";
            
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
            
            Call(port: 2);
        }

        private enum Level {
            Log,
            Warning,
            Assertion,
            Error
        }

    }

}