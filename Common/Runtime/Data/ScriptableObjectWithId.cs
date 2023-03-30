using UnityEngine;

namespace MisterGames.Common.Data {

    public class ScriptableObjectWithId : ScriptableObject {

        [SerializeField] [HideInInspector] private string _guid = "";
        
        public string Guid => _guid;

        private void Awake() {
            if (string.IsNullOrEmpty(_guid)) _guid = System.Guid.NewGuid().ToString();
        }
    }

}
