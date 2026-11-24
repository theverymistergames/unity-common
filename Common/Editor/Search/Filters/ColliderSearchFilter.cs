using System;
using UnityEngine;

namespace MisterGames.Common.Editor.Search.Filters {

    [Serializable]
    public sealed class ColliderSearchFilter : IObjectSearchFilter {

        public bool triggersOnly = true;
        public LayerMask gameObjectLayers = ~0;
        public LayerMask notExcludedLayers = 1;

        private const string IsTriggerField = "m_IsTrigger";
        private const string ExcludeLayersField = "m_ExcludeLayers.m_Bits";
        private const string BitsFieldName = "m_Bits";

        private static readonly int[] ColliderClassIds = {
            ObjectSearchClassIds.MeshCollider,
            ObjectSearchClassIds.BoxCollider,
            ObjectSearchClassIds.SphereCollider,
            ObjectSearchClassIds.CapsuleCollider,
            ObjectSearchClassIds.CharacterController,
            ObjectSearchClassIds.WheelCollider,
            ObjectSearchClassIds.TerrainCollider,
        };

        public void SetupRequest(AssetFileScanner.Request request) {
            for (int i = 0; i < ColliderClassIds.Length; i++) {
                request.classIds.Add(ColliderClassIds[i]);
            }

            request.fieldNames.Add(IsTriggerField);
            request.fieldNames.Add(BitsFieldName);
        }

        public bool IsMatch(AssetFileScanner.ScannedFile file, AssetFileScanner.ScannedObject obj) {
            return !obj.isGameObject &&
                   Array.IndexOf(ColliderClassIds, obj.classId) >= 0 &&
                   (!triggersOnly || GetInt(obj, IsTriggerField) != 0) &&
                   (gameObjectLayers.value & (1 << obj.gameObjectLayer)) != 0 &&
                   (GetUInt(obj, ExcludeLayersField) & unchecked((uint) notExcludedLayers.value)) == 0;
        }

        private static int GetInt(AssetFileScanner.ScannedObject obj, string fieldPath) {
            return obj.TryGetField(fieldPath, out var field) && int.TryParse(field.value, out int value) ? value : 0;
        }

        private static uint GetUInt(AssetFileScanner.ScannedObject obj, string fieldPath) {
            return obj.TryGetField(fieldPath, out var field) && uint.TryParse(field.value, out uint value) ? value : 0;
        }
    }

}
