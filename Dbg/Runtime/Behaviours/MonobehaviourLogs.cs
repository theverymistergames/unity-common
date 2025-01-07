using System;
using MisterGames.Common.Data;
using MisterGames.Common.GameObjects;
using UnityEngine;

namespace MisterGames.Dbg.Behaviours {
    
    public sealed class MonoBehaviourLogs : MonoBehaviour {
        
        [SerializeField] private Level _level;
        [SerializeField] private Prefix _prefix;
        [SerializeField] private Color _color = Color.white;
        [SerializeField] private Optional<string> _awake;
        [SerializeField] private Optional<string> _start;
        [SerializeField] private Optional<string> _enable;
        [SerializeField] private Optional<string> _disable;
        [SerializeField] private Optional<string> _destroy;

        private enum Prefix {
            Name,
            NameAndParent,
            PathInScene,
            None,
        }

        private enum Level {
            Info,
            Warning,
            Error,
            Exception,
        }
        
        private void Awake() {
            if (_awake.HasValue) Log($"Awake: {_awake.Value}");
        }

        private void Start() {
            if (_start.HasValue) Log($"Start: {_start.Value}");
        }

        private void OnEnable() {
            if (_enable.HasValue) Log($"OnEnable: {_enable.Value}");
        }

        private void OnDisable() {
            if (_disable.HasValue) Log($"OnDisable: {_disable.Value}");
        }

        private void OnDestroy() {
            if (_destroy.HasValue) Log($"OnDestroy: {_destroy.Value}");
        }

        private void Log(string message) {
            message = $"<color=yellow>[f {Time.frameCount}]</color> :: {GetPrefix()}<color=#{ColorUtility.ToHtmlStringRGB(_color)}>{message}</color>"; 
            
            switch (_level) {
                case Level.Info:
                    Debug.Log(message);
                    break;
                
                case Level.Warning:
                    Debug.LogWarning(message);
                    break;
                
                case Level.Error:
                    Debug.LogError(message);
                    break;
                
                case Level.Exception:
                    Debug.LogException(new Exception(message));
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private string GetPrefix() {
            return _prefix switch {
                Prefix.Name => $"{name} :: ",
                Prefix.NameAndParent => transform.parent != null ? $"{transform.parent.name}/{name} :: " : $"{name} :: ",
                Prefix.PathInScene => $"{transform.GetPathInScene(includeSceneName: true)} :: ",
                Prefix.None => "",
                _ => throw new ArgumentOutOfRangeException(),
            };
        }
    }
    
}