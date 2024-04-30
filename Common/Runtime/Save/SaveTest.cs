using UnityEngine;

namespace MisterGames.Common.Save {
    
    public sealed class SaveTest : MonoBehaviour, ISaveable {

        [SerializeField] private string _id;

        private int _idHash;
        
        private void OnEnable() {
            SaveSystem.Instance.Register(this, _id, out _idHash);
        }

        private void OnDisable() {
            SaveSystem.Instance.Unregister(this);
        }

        public void OnLoadData(ISaveSystem saveSystem) {
            saveSystem.Pop(_idHash, transform.position, out var position);
            transform.position = position;
        }

        public void OnSaveData(ISaveSystem saveSystem) {
            saveSystem.Push(_idHash, transform.position);
        }
    }
}