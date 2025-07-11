using System;
using System.Collections.Generic;
using MisterGames.Actors;
using MisterGames.Character.Motion;
using MisterGames.Common.GameObjects;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MisterGames.Character.Core {

    [DefaultExecutionOrder(-10000)]
    public sealed class CharacterSystem : MonoBehaviour {

        [SerializeField] private Actor _hero;

        public static CharacterSystem Instance { get; private set; }

        public IActor GetCharacter() {
            return _hero;
        }
        
        private void Awake() {
            Instance = this;

            if (_hero == null) {
                Debug.LogError($"{nameof(CharacterSystem)}: hero actor is not set. " +
                               $"Please assign a hero actor to the {nameof(CharacterSystem)} in the inspector " +
                               $"at {GameObjectExtensions.GetPathInScene(transform, includeSceneName: true)}.");
            }
        }
        
#if UNITY_EDITOR
        private void OnEnable() {
            if (TryTeleportToSpawnPoint()) return;
            
            SceneManager.activeSceneChanged += OnActiveSceneChanged;
        }

        private void OnDisable() {
            SceneManager.activeSceneChanged -= OnActiveSceneChanged;
        }

        private void OnActiveSceneChanged(Scene arg0, Scene arg1) {
            if (!TryTeleportToSpawnPoint()) return;
            
            SceneManager.activeSceneChanged -= OnActiveSceneChanged;
        }

        private bool TryTeleportToSpawnPoint() {
            if (_hero == null ||
                !_hero.TryGetComponent(out CharacterMotionPipeline motion) || motion.HasBeenTeleported ||
                FindObjectsByType<CharacterSpawnPoint>(FindObjectsInactive.Exclude, FindObjectsSortMode.None) is not { Length: > 0 } spawnPoints
            ) {
                return false;
            }
            
            Array.Sort(spawnPoints, (p0, p1) => p0.transform.GetInstanceID().CompareTo(p1.transform.GetInstanceID()));
            var spawnPoint = spawnPoints[0];
                
            spawnPoint.transform.GetPositionAndRotation(out var position, out var rotation);
            motion.Teleport(position, rotation, preserveVelocity: false);
            return true;
        }
#endif
    }

}
