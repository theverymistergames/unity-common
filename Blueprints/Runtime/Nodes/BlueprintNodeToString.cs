using System;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System.Linq;
#endif

namespace MisterGames.Blueprints.Nodes {

    [Serializable]
    [BlueprintNodeMeta(Name = "ToString", Category = "Debug", Color = BlueprintColors.Node.Debug)]
    public sealed class BlueprintNodeToString : BlueprintNode, IBlueprintOutput<string> {

        private Func<string> _getString = () => string.Empty;

        public override Port[] CreatePorts() => new[] {
            Port.Input(),
            Port.Output<string>()
        };

        public override void OnInitialize(IBlueprintHost host) {
            UnityEngine.Debug.LogWarning($"Using {nameof(BlueprintNodeToString)} " +
                                         $"in blueprint `{((BlueprintRunner) host.Runner).BlueprintAsset.name}` or in its subgraphs " +
                                         $"in blueprint runner `{host.Runner.name}`: " +
                                         $"this node uses reflection and is for debug purposes only, " +
                                         $"it must be removed in the release build.");

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            var inputPortLinks = RuntimePorts[0].links;
            if (inputPortLinks.Count == 0) return;

            var link = inputPortLinks[0];

            var interfaceTypeIBlueprintOutputT = link.node.GetType()
                .GetInterfaces()
                .FirstOrDefault(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IBlueprintOutput<>));

            if (interfaceTypeIBlueprintOutputT == null) return;

            var methodInfo = link.node.GetType().GetMethod("GetOutputPortValue");
            _getString = () => {
                object result = methodInfo!.Invoke(link.node, new object[] { link.port });
                return result == null ? string.Empty : result.ToString();
            };
#endif
        }

        public string GetOutputPortValue(int port) {
            if (port != 1) return default;

            return _getString.Invoke();
        }
    }

}
