using System;
using MisterGames.Actors;
using MisterGames.Common.Attributes;
using UnityEngine;

namespace MisterGames.Logic.Damage {
    
    public sealed class HealthBehaviour : MonoBehaviour, IActorComponent {

        [SerializeField] private bool _restoreFullHealthOnAwake;
        [SerializeField] [Min(0f)] private float _health;
        
        public delegate void DamageCallback(DamageInfo info);
        
        public event DamageCallback OnDamage = delegate { };
        public event Action OnDeath = delegate { };
        public event Action OnRestoreFullHealth = delegate { };
        
        public float Health => _health;
        public bool IsAlive => _health > 0f;
        public bool IsDead => _health <= 0f;
        
        private IActor _actor;
        private HealthData _healthData;

        void IActorComponent.OnAwake(IActor actor) {
            _actor = actor;
            if (_restoreFullHealthOnAwake) RestoreFullHealth(); 
        }

        void IActorComponent.OnSetData(IActor actor) {
            _healthData = actor.GetData<HealthData>();
        }

        public void RestoreFullHealth() {
            float oldHealth = _health;
            _health = _healthData?.health ?? _health;
            
            if (_health <= oldHealth) return;
            
            OnRestoreFullHealth.Invoke();
        }

        public DamageInfo Kill(IActor author = null, Vector3 point = default, bool notifyDamage = true) {
            float oldHealth = _health;
            _health = 0f;
            
            float damageTotal = oldHealth - _health;  
            var info = new DamageInfo(victim: _actor, damageTotal, mortal: true, author, point);

            if (oldHealth > 0f) {
                if (notifyDamage) OnDamage.Invoke(info);
                OnDeath.Invoke();   
            }
            
            return info;
        }
        
        public DamageInfo TakeDamage(float damage, IActor author = null, Vector3 point = default) {
            float oldHealth = _health;
            _health = Mathf.Max(0f, _health - damage);
            
            float damageTotal = oldHealth - _health;  
            bool mortal = _health <= 0;
            
            var info = new DamageInfo(victim: _actor, damageTotal, mortal, author, point);

            if (oldHealth > 0f) {
                OnDamage.Invoke(info);
                if (mortal) OnDeath.Invoke();   
            }
            
            return info;
        }

#if UNITY_EDITOR
        [Button] private void RestoreHealth() => RestoreFullHealth();
        [Button] private void KillHealth() => Kill(notifyDamage: true);
#endif
    }
    
}