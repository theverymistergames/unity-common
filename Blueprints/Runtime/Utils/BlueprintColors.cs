using UnityEngine;

namespace MisterGames.Blueprints {

    public static class BlueprintColors {
        
        public static class Node {
            public const string Flow = "#38697A";
            public const string Data = "#375D3B";
            public const string Time = "#37595D";
            public const string Actions = "#7E2D19";
                
            public const string Default = "#646464";
            public const string Blackboard = "#866B2B";
            public const string Exposed = "#5F436A";
        }

        public static class Port {

            public static class Names {
                    
                public const string Flow = "#68C2E1";
                public const string Data = "#80DD8A";

                public const string Bool = "#FFF690";
                public const string Number = "#80DD8A";
                public const string Vector = "#869AF5";
                public const string String = "#EBA467";
                public const string ScriptableObject = "#ECC3B5";
                public const string GameObject = "#F592A5";
                    
                public static string GetColorForType<T>() {
                    var t = typeof(T);
                
                    if (t == typeof(bool)) return Bool;
                    if (t == typeof(float) || t == typeof(int)) return Number;
                    if (t == typeof(string)) return String;
                    if (t == typeof(Vector2) || t == typeof(Vector3) || t == typeof(Vector4)) return Vector;
                    if (t == typeof(ScriptableObject)) return ScriptableObject;
                    if (t == typeof(GameObject)) return GameObject;

                    return Data;
                }
            }

            public static class Links {
                    
                public static readonly Color Flow = new Color(0.53f, 0.8f, 0.95f);
                public static readonly Color Data = new Color(0.45f, 0.87f, 0.49f);

                public static readonly Color Bool = new Color(0.93f, 0.89f, 0.4f);
                public static readonly Color Number = new Color(0.45f, 0.87f, 0.49f);
                public static readonly Color Vector = new Color(0.4f, 0.5f, 0.95f);
                public static readonly Color String = new Color(0.98f, 0.6f, 0.28f);
                public static readonly Color ScriptableObject = new Color(0.98f, 0.45f, 0.33f);
                public static readonly Color GameObject = new Color(0.8f, 0.3f, 0.4f);

                public static Color GetColorForType<T>() {
                    var t = typeof(T);
                
                    if (t == typeof(bool)) return Bool;
                    if (t == typeof(float) || t == typeof(int)) return Number;
                    if (t == typeof(string)) return String;
                    if (t == typeof(Vector2) || t == typeof(Vector3) || t == typeof(Vector4)) return Vector;
                    if (t == typeof(ScriptableObject)) return ScriptableObject;
                    if (t == typeof(GameObject)) return GameObject;

                    return Data;
                }
            }
            
        }
        
    }

}