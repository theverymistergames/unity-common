using UnityEditor;
using UnityEngine;

namespace MisterGames.Actors.Editor.Core {
    
    
    [CustomEditor(typeof(ActorData))]
    public sealed class ActorDataEditor : UnityEditor.Editor {

        private ActorData _data;
        private bool _isDirty;

        private void OnEnable() {
            if (_data != null) _data.OnValidateCalled -= OnValidateData;
            
            _data = target as ActorData;
            if (_data != null) _data.OnValidateCalled += OnValidateData;
        }

        private void OnDisable() {
            if (_data != null) _data.OnValidateCalled -= OnValidateData;
            _isDirty = false;
        }

        public override void OnInspectorGUI() {
            base.OnInspectorGUI();

            if (Application.isPlaying && _isDirty && _data != null && GUILayout.Button("Apply changes to actors")) {
                _data.NotifyValidateCalled();
                _isDirty = false;
            } 
        }

        private void OnValidateData() {
            _isDirty = true;
        }
    }
}