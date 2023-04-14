using System.Linq;
using MisterGames.Splines.Tools;
using UnityEditor;
using UnityEditor.Splines;
using UnityEngine.Splines;

namespace MisterGames.Splines.Editor {
    
    [CustomEditor(typeof(SplineMeshGenerator))]
    public class SplineMeshGeneratorEditor : UnityEditor.Editor {

        private SplineMeshGenerator[] _targets;

        private void OnEnable() {
            _targets = targets.Where(t => t is SplineMeshGenerator).Cast<SplineMeshGenerator>().ToArray();

            EditorSplineUtility.AfterSplineWasModified -= AfterSplineWasModified;
            EditorSplineUtility.AfterSplineWasModified += AfterSplineWasModified;
        }

        private void OnDisable() {
            EditorSplineUtility.AfterSplineWasModified -= AfterSplineWasModified;

            _targets = null;
        }

        private void AfterSplineWasModified(Spline spline) {
            if (_targets == null) return;

            for (int i = 0; i < _targets.Length; i++) {
                var splineMeshGenerator = _targets[i];
                if (!splineMeshGenerator.splineContainer.Splines.Contains(spline)) continue;

                splineMeshGenerator.TryGenerateMesh(splineMeshGenerator.splineContainer);
            }
        }
    }
    
}
