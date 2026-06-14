using System.Collections.Generic;
using UnityEngine;

namespace MisterGames.Actors {
    
    [CreateAssetMenu(fileName = nameof(ActorTemplate), menuName = "MisterGames/Actors/" + nameof(ActorTemplate))]
    public sealed class ActorTemplate : ScriptableObject {
        
        [SerializeField] private ActorTemplateLib _library;
        [SerializeReference] private List<IActorTemplate> _templates;

        private readonly SetBuilder<IActorTemplate> _dataBuilder = new();

        public void OnApplyInEditor(IActor actor) {
            var templates = BuildTemplateArray();
            for (int i = 0; i < templates?.Count; i++) {
                templates[i]?.OnApplyInEditor(actor, _library);
            }
        }

        public void OnApplyOnAwake(IActor actor) {
            var templates = BuildTemplateArray();
            for (int i = 0; i < templates?.Count; i++) {
                templates[i]?.OnApplyOnAwake(actor, _library);
            }
        }

        private IReadOnlyList<IActorTemplate> BuildTemplateArray() {
            _dataBuilder.Clear();
            
            if (_library != null) {
                _library.BuildTemplate(_dataBuilder);
            }

            _dataBuilder.Set(_templates);
            return _dataBuilder.GetResultArray();
        }

#if UNITY_EDITOR
        private void Reset() {
            OnValidate();
        }

        private void OnValidate() {
            var templates = BuildTemplateArray();

            int oldCount = _templates?.Count ?? 0;
            int newCount = templates.Count;

            bool replaceTemplates = oldCount != newCount;

            if (!replaceTemplates) {
                int minCount = Mathf.Min(oldCount, newCount);
                for (int i = 0; i < minCount; i++) {
                    var oldTemplate = _templates![i];
                    var newTemplate = templates[i];

                    if (oldTemplate?.GetType() == newTemplate?.GetType()) continue;

                    replaceTemplates = true;
                    break;
                }
            }

            if (replaceTemplates) {
                _templates ??= new List<IActorTemplate>();
                _templates.Clear();
                _templates.AddRange(templates);
            }

            for (int i = 0; i < _templates?.Count; i++) {
                _templates[i]?.OnValidate(_library);
            }
        }
#endif
    }
    
}