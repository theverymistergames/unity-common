using System;
using MisterGames.Scenes.Core;
using UnityEngine;

namespace MisterGames.Scenes.ActiveScene {

    [CreateAssetMenu(fileName = nameof(ActiveSceneSettings), menuName = "MisterGames/Scenes/" + nameof(ActiveSceneSettings))]
    public sealed class ActiveSceneSettings : ScriptableObject {
        
        [Header("Default")]
        public SceneReference[] neverSetActiveOnLoad;
        public SceneReference[] setActiveOnLoadHighPriority;
        public Mode defaultPriorityScenesMode;
        public SceneReference[] setActiveOnLoadLowPriority;
        
        [Header("Custom")]
        public ActiveSceneData[] customActiveScenes;
        
        [Serializable]
        public struct ActiveSceneData {
            public int priority;
            public SceneReference setActiveScene;
            public SceneReference[] forTheseScenes;
        }

        public enum Mode {
            DoNothing,
            SetLastLoadedAsActive,
        } 
    }
    
}