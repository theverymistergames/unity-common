#if DEVELOPMENT_BUILD || UNITY_EDITOR

using System;
using System.Linq;
using System.Text;

namespace MisterGames.Blueprints.Validation {

    internal static class ValidationUtils {

        public static Type GetGenericInterface(Type subjectType, Type interfaceType, Type genericArgumentType) {
            return subjectType
                .GetInterfaces()
                .FirstOrDefault(x =>
                    x.IsGenericType &&
                    x.GetGenericTypeDefinition() == interfaceType &&
                    x.GenericTypeArguments.Length == 1 &&
                    x.GenericTypeArguments[0] == genericArgumentType
                );
        }

        public static string RuntimeNodesToString(string prefix, BlueprintNode[] nodes) {
            var sb = new StringBuilder();

            sb.AppendLine(prefix);

            for (int n = 0; n < nodes.Length; n++) {
                var node = nodes[n];

                sb.AppendLine(RuntimeNodeToString(node));
            }

            return sb.ToString();
        }

        public static string RuntimeNodeToString(BlueprintNode node) {
            var sb = new StringBuilder();

            sb.AppendLine($"-- Node {node} (hash {node.GetHashCode()})");

            var ports = node.RuntimePorts;

            for (int p = 0; p < ports.Length; p++) {
                var port = ports[p];
                var links = port.links;

                if (links == null) {
                    sb.AppendLine($"---- Port#{p}: links: <null>");
                    continue;
                }

                if (links.Count == 0) {
                    sb.AppendLine($"---- Port#{p}: links: <empty>");
                    continue;
                }

                sb.AppendLine($"---- Port#{p}: links:");

                for (int l = 0; l < links.Count; l++) {
                    var link = links[l];

                    sb.AppendLine($"-------- Link#{l}: {link.node} (hash {link.node.GetHashCode()}) :: {link.port}");
                }
            }

            return sb.ToString();
        }
    }

}

#endif
