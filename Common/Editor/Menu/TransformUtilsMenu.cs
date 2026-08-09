using UnityEditor;
using UnityEngine;

namespace MisterGames.Common.Editor.Menu {
    
    public static class TransformUtilsMenu {

        private const string MovePositionKey = "TransformUtilsMenu_MovePosition";
        private const string MoveScaleKey = "TransformUtilsMenu_MoveScale";
        
        [MenuItem("CONTEXT/Transform/Move position to children")]
        public static void MovePosition(MenuCommand command) {
            if (command.context is not Transform root) return;

            var localPosition = root.localPosition;
            int childCount = root.childCount;
            
            for (int i = 0; i < childCount; i++) {
                var child = root.GetChild(i);
                
                Undo.RecordObject(child, MovePositionKey);
                child.localPosition += localPosition;
                EditorUtility.SetDirty(child);
            }

            Undo.RecordObject(root, MovePositionKey);
            root.localPosition = Vector3.zero;
            EditorUtility.SetDirty(root);
        }
        
        [MenuItem("CONTEXT/Transform/Move scale to children")]
        public static void MoveScale(MenuCommand command) {
            if (command.context is not Transform root) return;

            var rootScale = root.localScale;
            int childCount = root.childCount;

            for (int i = 0; i < childCount; i++) {
                var child = root.GetChild(i);

                Undo.RecordObject(child, MoveScaleKey);

                child.localPosition = Vector3.Scale(rootScale, child.localPosition);
                child.localScale = Vector3.Scale(GetScaleInLocalAxes(rootScale, child.localRotation), child.localScale);

                EditorUtility.SetDirty(child);
            }

            ScaleColliders(root, rootScale);

            Undo.RecordObject(root, MoveScaleKey);
            root.localScale = Vector3.one;
            EditorUtility.SetDirty(root);
        }

        /// <summary>
        /// Compensates colliders attached to the root itself: they lose the root scale,
        /// so their dimensions are baked into the collider settings where the collider type allows it.
        /// </summary>
        private static void ScaleColliders(Transform root, Vector3 scale) {
            var absScale = Abs(scale);
            var colliders = root.GetComponents<Collider>();

            for (int i = 0; i < colliders.Length; i++) {
                switch (colliders[i]) {
                    case BoxCollider box:
                        Undo.RecordObject(box, MoveScaleKey);
                        box.center = Vector3.Scale(scale, box.center);
                        box.size = Vector3.Scale(absScale, box.size);
                        EditorUtility.SetDirty(box);
                        break;

                    case SphereCollider sphere:
                        Undo.RecordObject(sphere, MoveScaleKey);
                        sphere.center = Vector3.Scale(scale, sphere.center);
                        sphere.radius *= MaxComponent(absScale);
                        EditorUtility.SetDirty(sphere);
                        break;

                    case CapsuleCollider capsule:
                        Undo.RecordObject(capsule, MoveScaleKey);
                        capsule.center = Vector3.Scale(scale, capsule.center);
                        capsule.height *= absScale[capsule.direction];
                        capsule.radius *= Mathf.Max(absScale[(capsule.direction + 1) % 3], absScale[(capsule.direction + 2) % 3]);
                        EditorUtility.SetDirty(capsule);
                        break;

                    case CharacterController controller:
                        Undo.RecordObject(controller, MoveScaleKey);
                        controller.center = Vector3.Scale(scale, controller.center);
                        controller.height *= absScale.y;
                        controller.radius *= Mathf.Max(absScale.x, absScale.z);
                        EditorUtility.SetDirty(controller);
                        break;

                    default:
                        Debug.LogWarning($"TransformUtilsMenu: cannot fit {colliders[i].GetType().Name} " +
                                         $"on '{root.name}' to the new scale, adjust it manually.", colliders[i]);
                        break;
                }
            }
        }

        private static Vector3 Abs(Vector3 v) {
            return new Vector3(Mathf.Abs(v.x), Mathf.Abs(v.y), Mathf.Abs(v.z));
        }

        private static float MaxComponent(Vector3 v) {
            return Mathf.Max(v.x, Mathf.Max(v.y, v.z));
        }

        /// <summary>
        /// Converts a scale defined in the parent axes into the scale along the axes of a child
        /// rotated by <paramref name="rotation"/>. Exact for uniform scale and for children rotated
        /// along the parent axes, approximate otherwise: an arbitrary rotation combined with a
        /// non uniform scale produces shear, which a Transform cannot represent.
        /// </summary>
        private static Vector3 GetScaleInLocalAxes(Vector3 scale, Quaternion rotation) {
            return new Vector3(
                GetAxisScale(scale, rotation * Vector3.right),
                GetAxisScale(scale, rotation * Vector3.up),
                GetAxisScale(scale, rotation * Vector3.forward)
            );
        }

        private static float GetAxisScale(Vector3 scale, Vector3 axis) {
            var scaledAxis = Vector3.Scale(scale, axis);
            return Mathf.Sign(Vector3.Dot(scaledAxis, axis)) * scaledAxis.magnitude;
        }
    }
    
}