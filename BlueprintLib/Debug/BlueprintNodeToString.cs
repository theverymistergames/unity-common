using System;
using MisterGames.Blueprints;
using MisterGames.Blueprints.Nodes;
using MisterGames.Common.Types;
using UnityEngine;
using System.Text;

namespace MisterGames.BlueprintLib {

    [Serializable]
    public class BlueprintSourceToString :
        BlueprintSource<BlueprintNodeToString>,
        BlueprintSources.IOutput<BlueprintNodeToString, string>,
        BlueprintSources.IConnectionCallback<BlueprintNodeToString>,
        BlueprintSources.ICloneable {}

    [Serializable]
    [BlueprintNode(Name = "ToString", Category = "Debug", Color = BlueprintColors.Node.Debug)]
    public struct BlueprintNodeToString :
        IBlueprintNode,
        IBlueprintOutput<string>,
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

        public void OnInitialize(IBlueprint blueprint, NodeToken token, NodeId root) {
            _getString = () => string.Empty;

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

#else
            Debug.LogWarning($"Using {nameof(BlueprintNodeToString2)} " +
                             $"in blueprint `{((BlueprintRunner2) blueprint.Host.Runner).BlueprintAsset}` or in its subgraphs " +
                             $"in blueprint runner `{blueprint.Host.Runner.name}`: " +
                             $"this node uses reflection and it is for debug purposes only, " +
                             $"it must be removed in the release build.\n" +
                             $"Note that in the non-development build it returns empty string.");
#endif
        }

        public string GetPortValue(IBlueprint blueprint, NodeToken token, int port) {
            return port == 1 ? _getString.Invoke() : default;
        }

        public void OnLinksChanged(IBlueprintMeta meta, NodeId id, int port) {
            if (port == 0) meta.InvalidateNode(id, invalidateLinks: false, notify: false);
        }
    }

}
