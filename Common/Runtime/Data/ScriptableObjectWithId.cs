using MisterGames.Common.Lists;
using UnityEngine;

namespace MisterGames.Common.Data {

    public class ScriptableObjectWithId : ScriptableObject {

        [SerializeField] [HideInInspector] private string _guid = "";
        
        public string Guid => _guid;

        protected virtual void Awake() {
            if (_guid.IsEmpty()) _guid = System.Guid.NewGuid().ToString();
        }
        
    }

}