using System;
using System.Collections.Generic;
using MisterGames.Common.Lists;
using Unity.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MisterGames.Common.Localization {
    
    [Serializable]
    public sealed class LocalizedStringRandomArrayProvider : ILocalizedStringProvider {

        public LocalizationKey[] keys;
        [Min(0)] public int selectCount;
        public bool allowRepeat;
        
        public void GetValues(List<LocalizationKey> buffer) {
            int keysCount = keys?.Length ?? 0;
            if (keysCount <= 0) return;
            
            if (allowRepeat) {
                for (int i = 0; i < selectCount; i++) {
                    buffer.Add(keys![Random.Range(0, keysCount)]);
                }
                return;
            }
            
            var indices = ArrayExtensions.Indices(keysCount, Allocator.Temp);
            
            for (int i = 0; i < selectCount; i++) {
                if (i % keysCount == 0) indices.Shuffle();
                buffer.Add(keys![indices[i]]);
            }
            
            indices.Dispose();
        }
    }
    
}