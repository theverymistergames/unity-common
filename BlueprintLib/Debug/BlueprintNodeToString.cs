using System;
using MisterGames.Blueprints;
using UnityEngine;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System.Text;
using System.Reflection;
using System.Linq;
#endif

namespace MisterGames.BlueprintLib {

    [Obsolete("Using BlueprintNodeToString, it must be removed in the release build!")]
    [Serializable]
    [BlueprintNodeMeta(Name = "ToString", Category = "Debug", Color = BlueprintColors.Node.Debug)]
    public sealed class BlueprintNodeToString : BlueprintNode, IBlueprintOutput<string> {

        private Func<string> _getString = () => string.Empty;

        public override Port[] CreatePorts() => new[] {
            Port.DynamicFunc(PortDirection.Input),
            Port.Func<string>(PortDirection.Output),
        };

        public override void OnInitialize(IBlueprintHost host) {
            Debug.LogWarning($"Using {nameof(BlueprintNodeToString)} " +
                             $"in blueprint `{((BlueprintRunner) host.Runner).BlueprintAsset.name}` or in its subgraphs " +
                             $"in blueprint runner `{host.Runner.name}`: " +
                             $"this node uses reflection and it is for debug purposes only, " +
                             $"it must be removed in the release build.\n" +
                             $"Note that in the non-development build it returns empty string.");

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Type GetGenericInterface(Type subjectType, Type interfaceType, params Type[] genericArguments) {
                return subjectType
                    .GetInterfaces()
                    .FirstOrDefault(x =>
                        x.IsGenericType &&
                        x.GetGenericTypeDefinition() == interfaceType &&
                        HasSameGenericTypeArguments(x.GenericTypeArguments, genericArguments)
                    );
            }

            bool HasSameGenericTypeArguments(Type[] argsA, Type[] argsB) {
                if (argsA.Length != argsB.Length) return false;

                for (int i = 0; i < argsA.Length; i++) {
                    if (argsA[i] != argsB[i]) return false;
                }

                return true;
            }

            var links = Ports[0].links;
            int linksCount = links.Count;
            if (linksCount == 0) return;

            var methods = new MethodInfo[linksCount];
            for (int l = 0; l < linksCount; l++) {
                var link = links[l];
                var port = link.node.CreatePorts()[link.port];

                var t = link.node.GetType();
                var interfaceType =
                    t.GetInterfaces().FirstOrDefault(x => x == typeof(IBlueprintOutput))
                    ?? GetGenericInterface(t, typeof(IBlueprintOutput<>), port.Signature.GetGenericArguments());

                methods[l] = interfaceType?.GetMethod("GetOutputPortValue");
            }

            _getString = () => {
                var sb = new StringBuilder();

                for (int l = 0; l < linksCount; l++) {
                    var link = links[l];
                    var method = methods[l];

                    string text = method == null
                        ? "<invalid link>"
                        : methods[l].Invoke(link.node, new object[] { link.port })?.ToString() ?? "<null>";

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
