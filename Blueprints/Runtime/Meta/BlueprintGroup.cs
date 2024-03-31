using System;
using System.Collections.Generic;
using UnityEngine;

namespace MisterGames.Blueprints.Meta {
    
    [Serializable]
    [BlueprintNode(Name = "Blueprint Group")]
    public struct BlueprintGroup {
        public int id;
        public string name;
        public Vector2 position;
        public List<NodeId> nodes;
    }
    
}