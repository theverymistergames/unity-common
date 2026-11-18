using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Common.Web;
using UnityEngine;

namespace MisterGames.ActionLib.Web {

    [Serializable]
    public sealed class OpenUrlAction : IActorAction {

        public UrlObject url;

        public UniTask Apply(IActor context, CancellationToken cancellationToken = default) {
            if (url != null) Application.OpenURL(url.Url);
            return default;
        }
    }

}
