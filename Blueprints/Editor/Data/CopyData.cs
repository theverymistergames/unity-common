using System;
using System.Collections.Generic;
using System.Linq;
using MisterGames.Blueprints.Core;
using MisterGames.Blueprints.Editor.Blueprints.Editor.Utils;
using MisterGames.Common.Lists;
using UnityEditor.Graphs;
using UnityEngine;

namespace MisterGames.Blueprints.Editor {

    public struct CopyData {

        public List<string> nodes;
        public List<string> links;
        public string position;

        public static string Serialize(List<BlueprintNode> nodes, List<CopyPasteLink> links) {
            if (nodes.Count == 0) return JsonUtility.ToJson(new CopyData());

            var copiedNodes = nodes
                .Select(n => new CopiedNode {
                    guid = n.Guid,
                    name = n.name,
                    position = n.AsIBlueprintNode().Position,
                    type = SerializedType.ToString(n.GetType())
                })
                .ToList();
                
            var nodesJson = copiedNodes
                .Select(n => JsonUtility.ToJson(n))
                .ToList();
            
            var linksJson = links
                .Select(l => JsonUtility.ToJson(l))
                .ToList();

            var position = copiedNodes
                .Select(n => n.position)
                .Aggregate((n1, n2) => n1 + n2) / copiedNodes.Count;

            string positionJson = JsonUtility.ToJson(position);
            
            var data = new CopyData { nodes = nodesJson, links = linksJson, position = positionJson };
            return JsonUtility.ToJson(data);
        }

    }

    public struct PasteData {
        
        public List<PastedNode> nodes;
        public List<CopyPasteLink> links;
        public Vector2 position;
        
        public static PasteData Deserialize(string data) {
            if (data == null || data.IsEmpty()) return new PasteData {
                nodes = new List<PastedNode>(),
                links = new List<CopyPasteLink>()
            };
            
            var copyData = JsonUtility.FromJson<CopyData>(data);
            return new PasteData {
                nodes = copyData.nodes
                    .Select(JsonUtility.FromJson<CopiedNode>)
                    .Select(copied => new PastedNode {
                        guid = copied.guid,
                        name = copied.name,
                        position = copied.position,
                        type = SerializedType.FromString(copied.type)
                    })
                    .ToList(),
                
                links = copyData.links
                    .Select(JsonUtility.FromJson<CopyPasteLink>)
                    .ToList(),
                
                position = JsonUtility.FromJson<Vector2>(copyData.position)
            };
        }
    }

    public struct PastedNode {
        public string guid;
        public string name;
        public Vector2 position;
        public Type type;
    }
    
    public struct CopiedNode {
        public string guid;
        public string name;
        public Vector2 position;
        public string type;
    }

    public struct CopyPasteLink {
        public string fromNodeGuid;
        public string toNodeGuid;
        public int fromPort;
        public int toPort;
    }

}