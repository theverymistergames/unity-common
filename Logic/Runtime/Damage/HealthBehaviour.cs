using System;
using MisterGames.Actors;
using MisterGames.Common.Attributes;
using UnityEngine;

namespace MisterGames.Logic.Damage {
    
    public sealed class HealthBehaviour : MonoBehaviour, IActorComponent {

        [SerializeField] private bool _restoreFullHealthOnAwake;

        public delegate void DamageCallback(DamageInfo info);
        
        public event DamageCallback OnDamage = delegate { };
        public event Action OnDeath = delegate { };
        public event Action OnRestoreHealth = delegate { };
        
        public float Health { get; private set; }
        public bool IsAlive => Health > 0f;
        public bool IsDead => Health <= 0f;
        
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
            float oldHealth = Health;
            Health = _healthData.health;
            
            if (Health <= oldHealth) return;
            
            OnRestoreHealth.Invoke();
        }

        public DamageInfo Kill(IActor author = null, Vector3 point = default, bool notifyDamage = true) {
            float oldHealth = Health;
            Health = 0f;
            
            float damageTotal = oldHealth - Health;  
            var info = new DamageInfo(victim: _actor, damageTotal, mortal: true, author, point);

            if (oldHealth > 0f) {
                if (notifyDamage) OnDamage.Invoke(info);
                OnDeath.Invoke();   
            }
            
            return info;
        }
        
        public DamageInfo TakeDamage(float damage, IActor author = null, Vector3 point = default) {
            float oldHealth = Health;
            Health = Mathf.Max(0f, Health - damage);
            
            float damageTotal = oldHealth - Health;  
            bool mortal = Health <= 0;
            
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