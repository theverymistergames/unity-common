using System;
using System.Collections.Generic;
using System.Threading;
using MisterGames.Common.Tick;

namespace MisterGames.Common.Data {
    
    public sealed class BlockSet {

        public event Action OnUpdate { add => _blocks.OnUpdate += value; remove => _blocks.OnUpdate -= value; }
        public int Count => _blocks.Count;
        
        private readonly CancelableSet<int> _blocks = new();
        
        public void SetBlock(object source, bool block, CancellationToken cancellationToken = default) {
            if (block) _blocks.Add(source.GetHashCode(), cancellationToken);
            else _blocks.Remove(source.GetHashCode());
        }

        public void Clear() {
            _blocks.Clear();
        }
    }
    
}