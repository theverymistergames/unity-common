using System.Collections.Generic;
using UnityEngine;

namespace MisterGames.Actors {
    
    [CreateAssetMenu(fileName = nameof(ActorTemplateLib), menuName = "MisterGames/Actors/" + nameof(ActorTemplateLib))]
    public class ActorTemplateLib : ScriptableObject {
        
        [Header("Default Template")]
        [SerializeReference] private List<IActorTemplate> _templates;

        public void BuildTemplate(ISetBuilder<IActorTemplate> builder) {
            builder.Set(_templates);
            OnBuildTemplate(builder);
        }

        protected virtual void OnBuildTemplate(ISetBuilder<IActorTemplate> builder) { }

#if UNITY_EDITOR
        private readonly SetBuilder<IActorTemplate> _dataBuilder = new();

        private void OnValidate() {
            _dataBuilder.Clear();
            BuildTemplate(_dataBuilder);

            var templates = _dataBuilder.GetResultArray();
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
                _templates[i]?.OnValidate(this);
            }
        }
#endif
    }
    
}