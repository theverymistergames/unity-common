using System;
using MisterGames.Blueprints;
using MisterGames.Blueprints.Meta;
using MisterGames.Blueprints.Nodes;
using MisterGames.Common.Types;
using UnityEngine;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System.Text;
using System.Reflection;
using System.Linq;
#endif

namespace MisterGames.BlueprintLib {

    [Serializable]
    public class BlueprintSourceToString :
        BlueprintSource<BlueprintNodeToString2>,
        BlueprintSources.IOutput<BlueprintNodeToString2, string>,
        BlueprintSources.IConnectionCallback<BlueprintNodeToString2>,
        BlueprintSources.ICloneable {}

    [Serializable]
    [BlueprintNode(Name = "ToString", Category = "Debug", Color = BlueprintColors.Node.Debug)]
    public struct BlueprintNodeToString2 :
        IBlueprintNode,
        IBlueprintOutput2<string>,
        IBlueprintConnectionCallback
    {
        [SerializeField] [HideInInspector] private SerializedType _dataType;

        private Func<string> _getString;

        public void CreatePorts(IBlueprintMeta meta, NodeId id) {
            Type dataType = null;

            if (meta.TryGetLinksFrom(id, 0, out int l)) {
                var link = meta.GetLink(l);
                dataType = meta.GetPort(link.id, link.port).DataType;
                _dataType = new SerializedType(dataType);
            }

            meta.AddPort(id, Port.DynamicInput(type: dataType));
            meta.AddPort(id, Port.Output<string>());
        }

        public void OnInitialize(IBlueprint blueprint, NodeToken token) {
            _getString = () => string.Empty;

            Debug.LogWarning($"Using {nameof(BlueprintNodeToString2)} " +
                             $"in blueprint `{((BlueprintRunner2) blueprint.Host.Runner).BlueprintAsset.name}` or in its subgraphs " +
                             $"in blueprint runner `{blueprint.Host.Runner.name}`: " +
                             $"this node uses reflection and it is for debug purposes only, " +
                             $"it must be removed in the release build.\n" +
                             $"Note that in the non-development build it returns empty string.");

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            var readMethod = typeof(IBlueprint).GetMethod("Read")?.MakeGenericMethod(_dataType.ToType());

            _getString = () => {
                object result = readMethod?.Invoke(blueprint, new object[] { token, 0, default });

                switch (result) {
                    case null:
                        return "<null>";

                    case Array array: {
                        int count = array.Length;

                        var sb = new StringBuilder();
                        sb.AppendLine($"{result} (count {count}){(count == 0 ? "" : ":")}");

                        for (int i = 0; i < count; i++) {
                            sb.AppendLine($"[{i}] {(array.GetValue(i)?.ToString() ?? "<null>")}");
                        }

                        return sb.ToString();
                    }

                    default:
                        return result.ToString();
                }
            };
#endif
        }

        public string GetPortValue(NodeToken token, int port) {
            return port == 1 ? _getString.Invoke() : default;
        }

        public void OnLinksChanged(IBlueprintMeta meta, NodeId id, int port) {
            if (port == 0) meta.InvalidateNode(id, invalidateLinks: false, notify: false);
        }
    }

    [Obsolete("Using BlueprintNodeToString, it must be removed in the release build!")]
    [Serializable]
    [BlueprintNodeMeta(Name = "ToString", Category = "Debug", Color = BlueprintColors.Node.Debug)]
    public sealed class BlueprintNodeToString :
        BlueprintNode,
        IBlueprintOutput<string>,
        IBlueprintPortLinksListener,
        IBlueprintPortDecorator
    {
        private Func<string> _getString = () => string.Empty;

        public override Port[] CreatePorts() => new[] {
            Port.DynamicInput(),
            Port.Output<string>(),
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
                var interfaceType = t.GetInterfaces().FirstOrDefault(x => x == typeof(IBlueprintOutput));

                if (interfaceType != null) {
                    methods[l] = interfaceType.GetMethod("GetOutputPortValue")?.MakeGenericMethod(port.DataType);
                    continue;
                }

                interfaceType = GetGenericInterface(t, typeof(IBlueprintOutput<>), port.DataType);
                if (interfaceType != null) {
                    methods[l] = interfaceType.GetMethod("GetOutputPortValue");
                    continue;
                }
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

#if UNITY_EDITOR
        public void DecoratePorts(BlueprintAsset blueprint, int nodeId, Port[] ports) {
            var linksFromInput = blueprint.BlueprintMeta.GetLinksFromNodePort(nodeId, 0);
            if (linksFromInput.Count == 0) return;

            var link = linksFromInput[0];
            var linkedPort = blueprint.BlueprintMeta.NodesMap[link.nodeId].Ports[link.portIndex];

            ports[0] = Port.DynamicInput(type: linkedPort.DataType);
        }

        public void OnPortLinksChanged(BlueprintAsset blueprint, int nodeId, int portIndex) {
            if (portIndex == 0) blueprint.BlueprintMeta.InvalidateNodePorts(blueprint, nodeId, invalidateLinks: false, notify: false);
        }
#endif
    }

}
