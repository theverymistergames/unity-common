using System;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System.Linq;
#endif

namespace MisterGames.Blueprints.Nodes {

    [Serializable]
    [BlueprintNodeMeta(Name = "ToString", Category = "Debug", Color = BlueprintColors.Node.Data)]
    public sealed class BlueprintNodeToString : BlueprintNode, IBlueprintOutput<string> {

        private Func<string> _getString = () => string.Empty;

        public override Port[] CreatePorts() => new[] {
            Port.Input(),
            Port.Output<string>()
        };

        public override void OnInitialize(IBlueprintHost host) {
            UnityEngine.Debug.LogWarning($"Using {nameof(BlueprintNodeToString)} in blueprint runner `{host.Runner.name}`: " +
                                         $"this node uses reflection and is for debug purposes only, " +
                                         $"it must be removed in the release build.");

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            // Search for first linked node that implements IBlueprintOutput<T> in order to catch real value.
            BlueprintNode node = this;
            int port = 0;

            while (true) {
                var inputPortLinks = node.RuntimePorts[port].links;
                if (inputPortLinks.Length == 0) {
                    node = null;
                    port = -1;
                    break;
                }

                var link = inputPortLinks[0];
                node = link.node;
                port = link.port;

                var interfaceTypeIBlueprintOutputT = node.GetType()
                    .GetInterfaces()
                    .FirstOrDefault(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IBlueprintOutput<>));

                if (interfaceTypeIBlueprintOutputT != null) break;
            }

            if (node == null) return;

            var methodInfo = node.GetType().GetMethod("GetPortValue");
            _getString = () => {
                object result = methodInfo!.Invoke(node, new object[] { port });
                return result == null ? string.Empty : result.ToString();
            };
#endif
        }

        public string GetPortValue(int port) {
            if (port != 1) return default;

            return _getString.Invoke();
        }
    }

}
