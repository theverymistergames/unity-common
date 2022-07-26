using UnityEngine;

namespace MisterGames.BlueprintLib {
    
    internal static class BlueprintLibColors {
        
        public static class Node {
            public const string Input = "#38697A";
            public const string Scenario = "#38697A";
            public const string Scenes = "#38697A";
        }

        public static class Port {
            public static class Names {
                public const string Input = "#68C2E1";
                public const string Scenario = "#68C2E1";
                public const string Scenes = "#68C2E1";
            }

            public static class Links {
                public static readonly Color Input = new Color(0.53f, 0.8f, 0.95f);
                public static readonly Color Scenario = new Color(0.53f, 0.8f, 0.95f);
                public static readonly Color Scenes = new Color(0.53f, 0.8f, 0.95f);
            }
        }
        
    }
    
}