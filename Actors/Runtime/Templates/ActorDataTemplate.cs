using System;
using UnityEngine;

namespace MisterGames.Actors
{
    
    [Serializable]
    public sealed class ActorDataTemplate : IActorTemplate
    {
        [SerializeReference] private IActorData[] _overrideData;

        void IActorTemplate.OnApplyInEditor(IActor actor, ActorTemplateLib lib)
        {
            actor.SetDataOverrides(this, _overrideData);
        }

        void IActorTemplate.OnApplyOnAwake(IActor actor, ActorTemplateLib lib)
        {
            actor.SetDataOverrides(this, _overrideData);
        }
    }
    
}