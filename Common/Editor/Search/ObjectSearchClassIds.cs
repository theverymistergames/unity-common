using System.Collections.Generic;

namespace MisterGames.Common.Editor.Search {

    public static class ObjectSearchClassIds {

        public const int GameObject = 1;
        public const int MeshCollider = 64;
        public const int BoxCollider = 65;
        public const int SphereCollider = 135;
        public const int CapsuleCollider = 136;
        public const int CharacterController = 143;
        public const int WheelCollider = 146;
        public const int TerrainCollider = 154;
        public const int MonoBehaviour = 114;

        private static readonly Dictionary<int, string> Names = new() {
            { GameObject, nameof(GameObject) },
            { MeshCollider, nameof(MeshCollider) },
            { BoxCollider, nameof(BoxCollider) },
            { SphereCollider, nameof(SphereCollider) },
            { CapsuleCollider, nameof(CapsuleCollider) },
            { CharacterController, nameof(CharacterController) },
            { WheelCollider, nameof(WheelCollider) },
            { TerrainCollider, nameof(TerrainCollider) },
            { MonoBehaviour, nameof(MonoBehaviour) },
        };

        public static string GetName(int classId) {
            return Names.TryGetValue(classId, out string name) ? name : $"ClassID {classId}";
        }
    }

}
