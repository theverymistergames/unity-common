using System;
using MisterGames.Tweens;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MisterGames.TweenLib.Animations {
    
    [Serializable]
    public sealed class SetBlendShapeProgressAction : ITweenProgressAction {
        
        public SkinnedMeshRenderer skinnedMeshRenderer;
        [Min(0)] public int index;
        [Range(0f, 100f)] public float startWeight;
        [Range(0f, 100f)] public float endWeight;
        
        public void OnProgressUpdate(float progress) {
            skinnedMeshRenderer.SetBlendShapeWeight(index, Mathf.Lerp(startWeight, endWeight, progress));

#if UNITY_EDITOR
            if (!Application.isPlaying) EditorUtility.SetDirty(skinnedMeshRenderer);
#endif
        }
    }
    
}