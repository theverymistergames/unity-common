using System.Collections;
using Unity.EditorCoroutines.Editor;

namespace MisterGames.Common.Editor.Coroutines {

    public class EditorCoroutineTask {
        
        private readonly object _owner;
        private readonly IEnumerator _coroutine;
        private EditorCoroutine _handle;

        public EditorCoroutineTask(object owner, IEnumerator coroutine) {
            _owner = owner;
            _coroutine = coroutine;
        }
        
        public void Start() {
            _handle = EditorCoroutineUtility.StartCoroutine(_coroutine, _owner);
        }
        
        public void Cancel() {
            EditorCoroutineUtility.StopCoroutine(_handle);
        }
        
    }

}