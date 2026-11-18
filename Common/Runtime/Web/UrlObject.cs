using UnityEngine;

namespace MisterGames.Common.Web {

    [CreateAssetMenu(fileName = nameof(UrlObject), menuName = "MisterGames/Web/" + nameof(UrlObject))]
    public sealed class UrlObject : ScriptableObject {

        [SerializeField] private string _url;

        public string Url => _url;
    }

}
