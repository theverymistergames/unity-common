using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MisterGames.Blueprints.Core {

    public interface IBlueprintResolver : IDisposable {
        
        BlueprintNode Resolve(BlueprintNode original);

        void Prepare(List<BlueprintNode> originals);
        
        void Prepare(BlueprintNode original);

    }

    internal sealed class BlueprintResolver : IBlueprintResolver {
            
        private Dictionary<string, BlueprintNode> runtimeNodes = new Dictionary<string, BlueprintNode>();

        public void Prepare(List<BlueprintNode> originals) {
            for (int i = 0; i < originals.Count; i++) {
                Prepare(originals[i]);
            } 
        }

        public void Prepare(BlueprintNode original) {
            var runtimeNode = Object.Instantiate(original);
            runtimeNodes[runtimeNode.Guid] = runtimeNode;
        }

        BlueprintNode IBlueprintResolver.Resolve(BlueprintNode original) {
            if (original == null) return null;

            if (runtimeNodes.ContainsKey(original.Guid)) {
                return runtimeNodes[original.Guid];
            }
            
            throw new ArgumentException($"Cannot find runtime copy of the original node [{original}]");
        }

        public void Dispose() {
            runtimeNodes.Clear();
            runtimeNodes = null;
        }
    }
    
}