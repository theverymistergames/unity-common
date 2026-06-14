using MisterGames.Common.Attributes;
using MisterGames.Common.GameObjects;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MisterGames.Actors
{
    
    [DisallowMultipleComponent]
    public sealed class ActorTemplater : MonoBehaviour, IActorComponent
    {
        [EmbeddedInspector]
        [SerializeField] private ActorTemplate _template;
        
        private IActor _actor;
        
        void IActorComponent.OnPostAwake(IActor actor)
        {
            _actor = actor;
            NotifyTemplateAwake();
        }

        public ActorTemplate GetTemplate()
        {
            return _template;
        }

        public void SetTemplate(ActorTemplate template)
        {
            _template = template;

#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
            if (CanApplyTemplateInEditor()) ApplyTemplateInEditor();
#endif
            
            if (_actor != null) NotifyTemplateAwake();
        }

        private void NotifyTemplateAwake()
        {
            _template?.OnApplyOnAwake(_actor);
        }

        [Button(showIf: nameof(CanApplyTemplateInEditor))]
        private void ApplyTemplateInEditor()
        {
            IActor actor = null;

            if (_template == null)
            {
                Debug.LogWarning($"{nameof(ActorTemplater)}.ApplyTemplateInEditor: f {Time.frameCount}, " +
                                 $"cannot apply null template for gameObject [{gameObject.GetPathInScene()}].");
                return;
            }
            
            if (_actor == null && !gameObject.TryGetComponent(out actor))
            {
                Debug.LogWarning($"{nameof(ActorTemplater)}.ApplyTemplateInEditor: f {Time.frameCount}, " +
                                 $"cannot apply template [{_template}] for gameObject [{gameObject.GetPathInScene()}]: " +
                                 $"cannot find actor on this gameObject");
                return;
            }
            
            _template.OnApplyInEditor(actor ?? _actor);
        }

        private bool CanApplyTemplateInEditor()
        {
            return !Application.isPlaying && _template != null;
        }
    }
    
}