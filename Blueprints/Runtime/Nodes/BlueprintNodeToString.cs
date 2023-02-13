using System;
using UnityEngine;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System.Text;
using System.Reflection;
using MisterGames.Blueprints.Validation;
#endif

namespace MisterGames.Blueprints.Nodes {

    [Obsolete("Using BlueprintNodeToString, it must be removed in the release build!")]
    [Serializable]
    [BlueprintNodeMeta(Name = "ToString", Category = "Debug", Color = BlueprintColors.Node.Debug)]
    public sealed class BlueprintNodeToString : BlueprintNode, IBlueprintOutput<string> {

        private Func<string> _getString = () => string.Empty;

#if UNITY_EDITOR
        public override Port[] CreatePorts() => new[] {
            Port.Input(),
            Port.Output<string>()
        };
#else
        public override Port[] CreatePorts() => null;
#endif

        public override void OnInitialize(IBlueprintHost host) {
            Debug.LogWarning($"Using {nameof(BlueprintNodeToString)} " +
                             $"in blueprint `{((BlueprintRunner) host.Runner).BlueprintAsset.name}` or in its subgraphs " +
                             $"in blueprint runner `{host.Runner.name}`: " +
                             $"this node uses reflection and it is for debug purposes only, " +
                             $"it must be removed in the release build.\n" +
                             $"Note that in the non-development build it returns empty string.");

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            var links = RuntimePorts[0].links;
            int linksCount = links.Count;
            if (linksCount == 0) return;

            var methods = new MethodInfo[linksCount];
            for (int l = 0; l < linksCount; l++) {
                var link = links[l];
                var method = link.node.GetType().GetMethod("GetOutputPortValue");

                if (method == null) {
                    var dataType = link.node.CreatePorts()[link.port].DataType;
                    var interfaceType = ValidationUtils.GetGenericInterface(
                        link.node.GetType(),
                        typeof(IBlueprintOutput<>),
                        dataType
                    );

                    method = interfaceType.GetMethod("GetOutputPortValue");
                }

                methods[l] = method;
            }

            _getString = () => {
                var sb = new StringBuilder();

                for (int l = 0; l < linksCount; l++) {
                    var link = links[l];

                    object output = methods[l].Invoke(link.node, new object[] { link.port });
                    string text = output == null ? "<null>" : output.ToString();

                    sb.Append($"{text}{(l < linksCount - 1 ? ", " : string.Empty)}");
                }

                return sb.ToString();
            };
#endif
        }

        public string GetOutputPortValue(int port) {
            return port == 1 ? _getString.Invoke() : default;
        }
    }

}
