using System;
using MisterGames.Common.Lists;
using UnityEditor;
using UnityEngine;

namespace MisterGames.Common.Editor.Menu {

    public class StringInputDialogEditorWindow : EditorWindow {

        private string _description;
        private string _inputText;
        private string _okButton;
        private string _cancelButton;

        private Action _onOkButton = delegate {  };

        private bool _initializedPosition = false;
        private bool _shouldClose = false;

        public static string Show(
            string title,
            string inputText,
            string description = "",
            string okButton = "Ok",
            string cancelButton = "Cancel"
        ) {
            string ret = null;
            var window = CreateInstance<StringInputDialogEditorWindow>();
            window.titleContent = new GUIContent(title);
            window._description = description;
            window._inputText = inputText;
            window._okButton = okButton;
            window._cancelButton = cancelButton;
            window._onOkButton = () => ret = window._inputText;
            window.ShowModal();
            return ret;
        }

        private void OnGUI() {
            var evt = Event.current;
            if (evt.type == EventType.KeyDown) {
                switch (evt.keyCode) {
                    // Escape pressed
                    case KeyCode.Escape:
                        _shouldClose = true;
                        break;

                    // Enter pressed
                    case KeyCode.Return:
                    case KeyCode.KeypadEnter:
                        _onOkButton.Invoke();
                        _shouldClose = true;
                        break;
                }
            }

            if (_shouldClose) {
                Close();
            }

            var rect = EditorGUILayout.BeginVertical();

            EditorGUILayout.Space(12); 
            EditorGUILayout.LabelField(_description);
            
            EditorGUILayout.Space(8);
            GUI.SetNextControlName("inText");
            _inputText = EditorGUILayout.TextField("", _inputText);
            GUI.FocusControl("inText"); // Focus text field
            EditorGUILayout.Space(12);

            // Draw OK / Cancel buttons
            var r = EditorGUILayout.GetControlRect();
            r.width /= 2;
            if (GUI.Button(r, _okButton)) {
                _onOkButton.Invoke();
                _shouldClose = true;
            }

            r.x += r.width;
            
            if (GUI.Button(r, _cancelButton)) {
                _inputText = null; // Cancel - delete inputText
                _shouldClose = true;
            }

            EditorGUILayout.Space(8);
            EditorGUILayout.EndVertical();

            if (Math.Abs(rect.width) > 0.1 && minSize != rect.size) {
                minSize = maxSize = rect.size;
            }

            // Set dialog position next to mouse position
            if (!_initializedPosition) {
                var mousePos = GUIUtility.GUIToScreenPoint(Event.current.mousePosition);
                position = new Rect(mousePos.x + 32, mousePos.y, position.width, position.height);
                _initializedPosition = true;
            }
        }
        
    }

}
