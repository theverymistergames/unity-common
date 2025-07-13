using System;
using MisterGames.Common.Attributes;
using MisterGames.Common.Labels;
using UnityEngine;

namespace MisterGames.Common.Service {
    
    [DefaultExecutionOrder(-10_000)]
    public sealed class ComponentServiceRegisterer : MonoBehaviour {
        
        [SerializeField] private Component _component;
        [SerializeField] private Lifetime _lifetime;
        [SerializeField] private ServiceType _serviceType;
        [VisibleIf(nameof(_serviceType), 1)]
        [SerializeField] private LabelValue _id;
        
        private enum Lifetime {
            AwakeDestroy,
            EnableDisable,
        }

        private enum ServiceType {
            Global,
            WithId,
        }

        private void Awake() {
            if (_lifetime == Lifetime.AwakeDestroy) Register();
        }

        private void OnDestroy() {
            if (_lifetime == Lifetime.AwakeDestroy) Unregister();
        }

        private void OnEnable() {
            if (_lifetime == Lifetime.EnableDisable) Register();
        }

        private void OnDisable() {
            if (_lifetime == Lifetime.EnableDisable) Unregister();
        }
        
        private void Register() {
            switch (_serviceType) {
                case ServiceType.Global:
                    Services.Register(_component, _component.GetType());
                    break;
                
                case ServiceType.WithId:
                    Services.Register(_component, _component.GetType(), _id.GetValue());
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        private void Unregister() {
            switch (_serviceType) {
                case ServiceType.Global:
                    Services.Unregister(_component);
                    break;
                
                case ServiceType.WithId:
                    Services.Unregister(_component, _id.GetValue());
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
    
}