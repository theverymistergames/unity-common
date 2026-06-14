using System;
using UnityEngine;

namespace MisterGames.Actors
{
    
    [Serializable]
    public abstract class ActorDataAsTemplate<T> : IActorTemplate where T : class, IActorData
    {
        [SerializeField] private T _data;

        void IActorTemplate.OnApplyInEditor(IActor actor, ActorTemplateLib lib)
        {
            actor.SetDataOverride(this, _data);
        }

        void IActorTemplate.OnApplyOnAwake(IActor actor, ActorTemplateLib lib)
        {
            actor.SetDataOverride(this, _data);
        }
    }
    
}