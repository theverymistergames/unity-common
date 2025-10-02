using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using MisterGames.Common.Data;
using MisterGames.Common.Strings;
using MisterGames.Common.Tick;
using MisterGames.Input.Actions;
using Unity.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MisterGames.Input.Core {

    [Serializable]
    public sealed class InputBlockService : IInputBlockService, IDisposable, IUpdate {

        [SerializeField] private bool _enableLogs;
        
        private readonly struct BlockKey : IEquatable<BlockKey> {
            
            public readonly int hash;
            public readonly Guid guid;
            
            public BlockKey(int hash, Guid guid) {
                this.hash = hash;
                this.guid = guid;
            }
            
            public bool Equals(BlockKey other) => hash == other.hash && guid.Equals(other.guid);
            public override bool Equals(object obj) => obj is BlockKey other && Equals(other);
            public override int GetHashCode() => HashCode.Combine(hash, guid);
            public static bool operator ==(BlockKey left, BlockKey right) => left.Equals(right);
            public static bool operator !=(BlockKey left, BlockKey right) => !left.Equals(right);
        }
        
        private readonly struct BlockData {
            
            public readonly bool blocked;
            public readonly CancellationToken cancellationToken;
            
            public BlockData(bool blocked, CancellationToken cancellationToken) {
                this.blocked = blocked;
                this.cancellationToken = cancellationToken;
            }
        }

        private readonly MultiValueDictionary<int, Guid> _sourceHashToInputActionBlocks = new();
        private readonly MultiValueDictionary<Guid, int> _inputActionToSourceHashBlocks = new();
        private readonly Dictionary<BlockKey, BlockData> _inputActionBlocks = new();
        
        private readonly MultiValueDictionary<int, Guid> _sourceHashToInputMapBlocks = new();
        private readonly MultiValueDictionary<Guid, int> _inputMapToSourceHashBlocks = new();
        private readonly Dictionary<BlockKey, BlockData> _inputMapBlocks = new();
        
        private IInputMapper _inputMapper;
        private bool _initialized;
        
        public void Initialize(IInputMapper inputMapper) {
            _initialized = true;
            _inputMapper = inputMapper;
            
            PlayerLoopStage.LateUpdate.Subscribe(this);
        }
        
        public void Dispose() {
            if (!_initialized) return;

            _initialized = false;
            
            PlayerLoopStage.LateUpdate.Unsubscribe(this);
            
            _sourceHashToInputActionBlocks.Clear();
            _inputActionToSourceHashBlocks.Clear();
            _inputActionBlocks.Clear();
            
            _sourceHashToInputMapBlocks.Clear();
            _inputMapToSourceHashBlocks.Clear();
            _inputMapBlocks.Clear();
        }

        void IUpdate.OnUpdate(float dt) {
            var inputActionBlocksToRemove = new NativeArray<BlockKey>(_inputActionBlocks.Count, Allocator.Temp);
            var inputMapBlocksToRemove = new NativeArray<BlockKey>(_inputMapBlocks.Count, Allocator.Temp);
            
            var changedInputActions = new NativeHashSet<Guid>(_inputActionBlocks.Count, Allocator.Temp);
            var changedInputMaps = new NativeHashSet<Guid>(_inputMapBlocks.Count, Allocator.Temp);
            
            int removeInputActionsCount = 0;
            int removeInputMapsCount = 0;
            
            foreach (var (key, data) in _inputActionBlocks) {
                if (data.cancellationToken.IsCancellationRequested) inputActionBlocksToRemove[removeInputActionsCount++] = key;
            }
            
            foreach (var (key, data) in _inputMapBlocks) {
                if (data.cancellationToken.IsCancellationRequested) inputMapBlocksToRemove[removeInputMapsCount++] = key;
            }
            
            for (int i = 0; i < removeInputActionsCount; i++) {
                var key = inputActionBlocksToRemove[i];
                
                _inputActionBlocks.Remove(key);
                _sourceHashToInputActionBlocks.RemoveValue(key.hash, key.guid);
                _inputActionToSourceHashBlocks.RemoveValue(key.guid, key.hash);

                changedInputActions.Add(key.guid);
            }
            
            inputActionBlocksToRemove.Dispose();

            for (int i = 0; i < removeInputMapsCount; i++) {
                var key = inputMapBlocksToRemove[i];
                
                _inputMapBlocks.Remove(key);
                _sourceHashToInputMapBlocks.RemoveValue(key.hash, key.guid);
                _inputMapToSourceHashBlocks.RemoveValue(key.guid, key.hash);
                
                changedInputMaps.Add(key.guid);
            }
            
            inputMapBlocksToRemove.Dispose();
            
            foreach (var guid in changedInputMaps) {
                UpdateInputMapBlockState(_inputMapper.GetInputMap(guid));
            }
            
            foreach (var guid in changedInputActions) {
                var inputAction = _inputMapper.GetInputAction(guid);

                if (inputAction == null ||
                    inputAction.actionMap != null && changedInputMaps.Contains(inputAction.actionMap.id)) 
                {
                    continue;
                }
                
                UpdateInputActionBlockState(inputAction);
            }

            if (_enableLogs && (changedInputActions.Count > 0 || changedInputMaps.Count > 0)) {
                LogInfo($"cleared canceled blocks for maps [{JoinMapsToString(changedInputMaps)}] and inputs [{JoinActionsToString(changedInputActions)}], state:\n{GetInputsStateString()}");
            }
            
            changedInputActions.Dispose();
            changedInputMaps.Dispose();
        }

        public bool IsInputActionEnabled(InputAction inputAction) {
            return _initialized && inputAction is { enabled: true } && IsInputMapEnabled(inputAction.actionMap);
        }
        
        public bool IsInputMapEnabled(InputActionMap inputMap) {
            return _initialized && inputMap is { enabled: true } && !_inputMapToSourceHashBlocks.ContainsKey(inputMap.id);
        }

        #region InputMapBlocks
        
        public bool BlockInputMap(object source, InputActionMap inputMap, CancellationToken cancellationToken = default) {
            if (_initialized && SetInputMapBlockInternal(source, inputMap, blocked: true, cancellationToken)) {
                UpdateInputMapBlockState(inputMap);
                if (_enableLogs) LogInfo($"map [{inputMap.name}] blocked by [{source}], state:\n{GetInputsStateString()}");
                return true;
            }
            
            return false;
        }
        
        public bool BlockInputMap(object source, InputMapRef inputMapRef, CancellationToken cancellationToken = default) {
            if (_initialized && _inputMapper.GetInputMap(inputMapRef.Guid) is { } inputMap && 
                SetInputMapBlockInternal(source, inputMap, blocked: true, cancellationToken)) 
            {
                UpdateInputMapBlockState(inputMap);
                if (_enableLogs) LogInfo($"map [{inputMap.name}] blocked by [{source}], state:\n{GetInputsStateString()}");
                return true;
            }
            
            return false;
        }

        public bool BlockInputMaps(object source, IReadOnlyList<InputActionMap> inputMaps, CancellationToken cancellationToken = default) {
            if (!_initialized) return false;
            
            var changedList = new NativeList<Guid>(Allocator.Temp);
            
            for (int i = 0; i < inputMaps?.Count; i++) {
                var inputMap = inputMaps[i];
                
                if (SetInputMapBlockInternal(source, inputMap, blocked: true, cancellationToken)) {
                    changedList.Add(inputMap.id);
                }
            }

            int count = changedList.Length;
            if (count > 0) {
                UpdateInputMapsBlockState(changedList);
                if (_enableLogs) LogInfo($"maps [{JoinMapsToString(inputMaps)}] blocked by [{source}], state:\n{GetInputsStateString()}");
            }
            
            changedList.Dispose();
            
            return count > 0;
        }

        public bool BlockInputMaps(object source, IReadOnlyList<InputMapRef> inputMapRefs, CancellationToken cancellationToken = default) {
            if (!_initialized) return false;
            
            var changedList = new NativeList<Guid>(Allocator.Temp);
            
            for (int i = 0; i < inputMapRefs?.Count; i++) {
                var inputMap = _inputMapper.GetInputMap(inputMapRefs[i].Guid);
                
                if (SetInputMapBlockInternal(source, inputMap, blocked: true, cancellationToken)) {
                    changedList.Add(inputMap.id);
                }
            }

            int count = changedList.Length;
            if (count > 0) {
                UpdateInputMapsBlockState(changedList);
                if (_enableLogs) LogInfo($"maps [{JoinMapsToString(inputMapRefs)}] blocked by [{source}], state:\n{GetInputsStateString()}");
            }
            
            changedList.Dispose();
            
            return count > 0;
        }
        
        public bool UnblockInputMap(object source, InputActionMap inputMap) {
            if (_initialized && SetInputMapBlockInternal(source, inputMap, blocked: false)) {
                UpdateInputMapBlockState(inputMap);
                if (_enableLogs) LogInfo($"map [{inputMap.name}] unblocked by [{source}], state:\n{GetInputsStateString()}");
                return true;
            }
            
            return false;
        }
        
        public bool UnblockInputMap(object source, InputMapRef inputMapRef) {
            if (_initialized &&
                _inputMapper.GetInputMap(inputMapRef.Guid) is { } inputMap &&
                SetInputMapBlockInternal(source, inputMap, blocked: false)) 
            {
                UpdateInputMapBlockState(inputMap);
                if (_enableLogs) LogInfo($"map [{inputMap.name}] unblocked by [{source}], state:\n{GetInputsStateString()}");
                return true;
            }
            
            return false;
        }

        public bool UnblockInputMaps(object source, IReadOnlyList<InputActionMap> inputMaps) {
            if (!_initialized) return false;
            
            var changedList = new NativeList<Guid>(Allocator.Temp);
            
            for (int i = 0; i < inputMaps?.Count; i++) {
                var inputMap = inputMaps[i];
                
                if (SetInputMapBlockInternal(source, inputMap, blocked: false)) {
                    changedList.Add(inputMap.id);
                }
            }

            int count = changedList.Length;
            if (count > 0) {
                UpdateInputMapsBlockState(changedList);
                if (_enableLogs) LogInfo($"maps [{JoinMapsToString(inputMaps)}] unblocked by [{source}], state:\n{GetInputsStateString()}");
            }
            
            changedList.Dispose();
            
            return count > 0;
        }
        
        public bool UnblockInputMaps(object source, IReadOnlyList<InputMapRef> inputMapRefs) {
            if (!_initialized) return false;
            
            var changedList = new NativeList<Guid>(Allocator.Temp);
            
            for (int i = 0; i < inputMapRefs?.Count; i++) {
                var inputMap = _inputMapper.GetInputMap(inputMapRefs[i].Guid);
                
                if (SetInputMapBlockInternal(source, inputMap, blocked: false)) {
                    changedList.Add(inputMap.id);
                }
            }

            int count = changedList.Length;
            if (count > 0) {
                UpdateInputMapsBlockState(changedList);
                if (_enableLogs) LogInfo($"maps [{JoinMapsToString(inputMapRefs)}] unblocked by [{source}], state:\n{GetInputsStateString()}");
            }
            
            changedList.Dispose();
            
            return count > 0;
        }

        public bool ClearAllInputMapBlocksOf(object source) {
            if (!_initialized) return false;
            
            int hash = source.GetHashCode();
            int count = _sourceHashToInputMapBlocks.GetCount(hash);

            if (count <= 0) return false;
            
            var changedList = new NativeList<Guid>(Allocator.Temp);
            
            for (int i = count - 1; i >= 0; i--) {
                if (!_sourceHashToInputMapBlocks.TryGetValueAt(hash, i, out var guid)) continue;
                
                _inputMapToSourceHashBlocks.RemoveValue(guid, hash);
                    
                if (_inputMapBlocks.Remove(new BlockKey(hash, guid))) changedList.Add(guid);
            }

            _sourceHashToInputMapBlocks.RemoveValues(hash);
            
            int changedCount = changedList.Length;
            if (changedCount > 0) {
                UpdateInputMapsBlockState(changedList);
                if (_enableLogs) LogInfo($"cleared all {changedCount} map blocks of [{source}], state:\n{GetInputsStateString()}");
            }
            
            changedList.Dispose();
            
            return changedCount > 0;
        }

        public bool ClearAllInputMapBlocks() {
            if (!_initialized) return false;
            
            int changedCount = _inputMapBlocks.Count;
            var changedList = new NativeList<Guid>(changedCount, Allocator.Temp);
            
            foreach (var key in _inputMapBlocks.Keys) {
                changedList.Add(key.guid);
            }
            
            _inputMapBlocks.Clear();
            _sourceHashToInputMapBlocks.Clear();
            _inputMapToSourceHashBlocks.Clear();

            if (changedCount > 0) {
                UpdateInputMapsBlockState(changedList);
                if (_enableLogs) LogInfo($"cleared all {changedCount} map blocks of all sources, state:\n{GetInputsStateString()}");
            }
            
            changedList.Dispose();
            
            return changedCount > 0;
        }
        
        private bool SetInputMapBlockInternal(object source, InputActionMap inputMap, bool blocked, CancellationToken cancellationToken = default) {
            if (!_initialized || inputMap == null) return false;
            
            var guid = inputMap.id;
            int hash = source.GetHashCode();

            if (!blocked || cancellationToken.IsCancellationRequested) {
                _sourceHashToInputMapBlocks.RemoveValue(hash, guid);
                _inputMapToSourceHashBlocks.RemoveValue(guid, hash);
                
                return _inputMapBlocks.Remove(new BlockKey(hash, guid));
            }

            if (!_sourceHashToInputMapBlocks.ContainsValue(hash, guid)) {
                _sourceHashToInputMapBlocks.AddValue(hash, guid);
            }
            
            if (!_inputMapToSourceHashBlocks.ContainsValue(guid, hash)) {
                _inputMapToSourceHashBlocks.AddValue(guid, hash);
            }
            
            _inputMapBlocks[new BlockKey(hash, guid)] = new BlockData(blocked: true, cancellationToken);
            return true;
        }

        #endregion
        
        #region InputActionBlockOverrides
        
        public bool SetInputActionBlockOverride(object source, InputAction inputAction, bool blocked, CancellationToken cancellationToken = default) {
            if (_initialized && SetInputActionBlockOverrideInternal(source, inputAction, blocked, cancellationToken)) {
                UpdateInputActionBlockState(inputAction);
                if (_enableLogs) LogInfo($"input [{inputAction.name}] set overriden state {(blocked ? "blocked" : "unblocked")} by [{source}], state:\n{GetInputsStateString()}");
                return true;
            }
            
            return false;
        }

        public bool SetInputActionBlockOverride(object source, InputActionRef inputActionRef, bool blocked, CancellationToken cancellationToken = default) {
            if (_initialized && 
                _inputMapper.GetInputAction(inputActionRef.Guid) is { } inputAction &&
                SetInputActionBlockOverrideInternal(source, inputAction, blocked, cancellationToken)) 
            {
                UpdateInputActionBlockState(inputAction);
                if (_enableLogs) LogInfo($"input [{inputAction.name}] set overriden state {(blocked ? "blocked" : "unblocked")} by [{source}], state:\n{GetInputsStateString()}");
                return true;
            }
            
            return false;
        }

        public bool SetInputActionBlockOverrides(object source, IReadOnlyList<InputAction> inputActions, bool blocked, CancellationToken cancellationToken = default) {
            if (!_initialized) return false;
            
            var changedList = new NativeList<Guid>(Allocator.Temp);
            
            for (int i = 0; i < inputActions?.Count; i++) {
                var inputAction = inputActions[i];
                
                if (SetInputActionBlockOverrideInternal(source, inputAction, blocked, cancellationToken)) {
                    changedList.Add(inputAction.id);
                }
            }

            int count = changedList.Length;
            if (count > 0) {
                UpdateInputActionsBlockState(changedList);
                if (_enableLogs) LogInfo($"inputs [{JoinActionsToString(inputActions)}] set overriden state {(blocked ? "blocked" : "unblocked")} by [{source}], state:\n{GetInputsStateString()}");
            }
            
            changedList.Dispose();
            
            return count > 0;
        }

        public bool SetInputActionBlockOverrides(object source, IReadOnlyList<InputActionRef> inputActionRefs, bool blocked, CancellationToken cancellationToken = default) {
            if (!_initialized) return false;
            
            var changedList = new NativeList<Guid>(Allocator.Temp);
            
            for (int i = 0; i < inputActionRefs?.Count; i++) {
                var inputAction = _inputMapper.GetInputAction(inputActionRefs[i].Guid);
                
                if (SetInputActionBlockOverrideInternal(source, inputAction, blocked, cancellationToken)) {
                    changedList.Add(inputAction.id);
                }
            }

            int count = changedList.Length;
            if (count > 0) {
                UpdateInputActionsBlockState(changedList);
                if (_enableLogs) LogInfo($"inputs [{JoinActionsToString(inputActionRefs)}] set overriden state {(blocked ? "blocked" : "unblocked")} by [{source}], state:\n{GetInputsStateString()}");
            }
            
            changedList.Dispose();
            
            return count > 0;
        }

        public bool RemoveInputActionBlockOverride(object source, InputAction inputAction) {
            if (_initialized && RemoveInputActionBlockOverrideInternal(source, inputAction)) {
                UpdateInputActionBlockState(inputAction);
                if (_enableLogs) LogInfo($"input [{inputAction.name}] removed overriden state by [{source}], state:\n{GetInputsStateString()}");
                return true;
            }
            
            return false;
        }
        
        public bool RemoveInputActionBlockOverride(object source, InputActionRef inputActionRef) {
            var inputAction = _inputMapper.GetInputAction(inputActionRef.Guid);
            
            if (_initialized && RemoveInputActionBlockOverrideInternal(source, inputAction)) {
                UpdateInputActionBlockState(inputAction);
                if (_enableLogs) LogInfo($"input [{inputAction.name}] removed overriden state by [{source}], state:\n{GetInputsStateString()}");
                return true;
            }
            
            return false;
        }

        public bool RemoveInputActionBlockOverrides(object source, IReadOnlyList<InputAction> inputActions) {
            if (!_initialized) return false;
            
            var changedList = new NativeList<Guid>(Allocator.Temp);
            
            for (int i = 0; i < inputActions?.Count; i++) {
                var inputAction = inputActions[i];
                
                if (RemoveInputActionBlockOverrideInternal(source, inputAction)) {
                    changedList.Add(inputAction.id);
                }
            }

            int count = changedList.Length;
            if (count > 0) {
                UpdateInputActionsBlockState(changedList);
                if (_enableLogs) LogInfo($"inputs [{JoinActionsToString(inputActions)}] removed overriden state by [{source}], state:\n{GetInputsStateString()}");
            }
            
            changedList.Dispose();
            
            return count > 0;
        }
        
        public bool RemoveInputActionBlockOverrides(object source, IReadOnlyList<InputActionRef> inputActionRefs) {
            if (!_initialized) return false;
            
            var changedList = new NativeList<Guid>(Allocator.Temp);
            
            for (int i = 0; i < inputActionRefs?.Count; i++) {
                var inputAction = _inputMapper.GetInputAction(inputActionRefs[i].Guid);
                
                if (RemoveInputActionBlockOverrideInternal(source, inputAction)) {
                    changedList.Add(inputAction.id);
                }
            }

            int count = changedList.Length;
            if (count > 0) {
                UpdateInputActionsBlockState(changedList);
                if (_enableLogs) LogInfo($"inputs [{JoinActionsToString(inputActionRefs)}] removed overriden state by [{source}], state:\n{GetInputsStateString()}");
            }
            
            changedList.Dispose();
            
            return count > 0;
        }
        
        public bool ClearAllInputActionBlockOverridesOf(object source) {
            if (!_initialized) return false;
            
            int hash = source.GetHashCode();
            int count = _sourceHashToInputActionBlocks.GetCount(hash);

            if (count <= 0) return false;
            
            var changedList = new NativeList<Guid>(Allocator.Temp);
            
            for (int i = count - 1; i >= 0; i--) {
                if (!_sourceHashToInputActionBlocks.TryGetValueAt(hash, i, out var guid)) continue;
                
                _inputActionToSourceHashBlocks.RemoveValue(guid, hash);
                    
                if (_inputActionBlocks.Remove(new BlockKey(hash, guid))) changedList.Add(guid);
            }

            _sourceHashToInputActionBlocks.RemoveValues(hash);
            
            int changedCount = changedList.Length;
            if (changedCount > 0) {
                UpdateInputActionsBlockState(changedList);
                if (_enableLogs) LogInfo($"cleared all {changedCount} input overriden states of [{source}], state:\n{GetInputsStateString()}");
            }
            
            changedList.Dispose();
            
            return changedCount > 0;
        }
        
        public bool ClearAllInputActionBlockOverrides() {
            if (!_initialized) return false;
            
            int changedCount = _inputActionBlocks.Count;
            var changedList = new NativeList<Guid>(changedCount, Allocator.Temp);

            foreach (var key in _inputActionBlocks.Keys) {
                changedList.Add(key.guid);
            }
            
            _inputActionBlocks.Clear();
            _sourceHashToInputActionBlocks.Clear();
            _inputActionToSourceHashBlocks.Clear();

            if (changedCount > 0) {
                UpdateInputActionsBlockState(changedList);
                if (_enableLogs) LogInfo($"cleared all {changedCount} input overriden states of all sources, state:\n{GetInputsStateString()}");
            }
            
            changedList.Dispose();
            
            return changedCount > 0;
        }
        
        private bool SetInputActionBlockOverrideInternal(object source, InputAction inputAction, bool blocked, CancellationToken cancellationToken) {
            if (!_initialized || inputAction == null) return false;
            
            var guid = inputAction.id;
            int hash = source.GetHashCode();

            if (cancellationToken.IsCancellationRequested) {
                _sourceHashToInputActionBlocks.RemoveValue(hash, guid);
                _inputActionToSourceHashBlocks.RemoveValue(guid, hash);
                
                return _inputActionBlocks.Remove(new BlockKey(hash, guid));
            }

            if (!_sourceHashToInputActionBlocks.ContainsValue(hash, guid)) {
                _sourceHashToInputActionBlocks.AddValue(hash, guid);
            }
            
            if (!_inputActionToSourceHashBlocks.ContainsValue(guid, hash)) {
                _inputActionToSourceHashBlocks.AddValue(guid, hash);
            }
            
            _inputActionBlocks[new BlockKey(hash, guid)] = new BlockData(blocked, cancellationToken);
            return true;
        }
        
        private bool RemoveInputActionBlockOverrideInternal(object source, InputAction inputAction) {
            if (!_initialized || inputAction == null) return false;
            
            var guid = inputAction.id;
            int hash = source.GetHashCode();

            _sourceHashToInputActionBlocks.RemoveValue(hash, guid);
            _inputActionToSourceHashBlocks.RemoveValue(guid, hash);
            
            return _inputActionBlocks.Remove(new BlockKey(hash, guid));
        }
        
        #endregion

        public void ClearAllBlocks() {
            if (!_initialized) return;
            
            _sourceHashToInputActionBlocks.Clear();
            _inputActionToSourceHashBlocks.Clear();
            _inputActionBlocks.Clear();
            
            _sourceHashToInputMapBlocks.Clear();
            _inputMapToSourceHashBlocks.Clear();
            _inputMapBlocks.Clear();

            for (int i = 0; i < _inputMapper.InputMaps.Count; i++) {
                var inputMap = _inputMapper.InputMaps[i];
                inputMap.Enable();

                var inputActions = inputMap.actions;
                for (int j = 0; j < inputActions.Count; j++) {
                    inputMap.actions[j].Enable();
                }
            }
        }
        
        private void UpdateInputMapBlockState(InputActionMap inputMap) {
            for (int i = 0; i < inputMap?.actions.Count; i++) {
                UpdateInputActionBlockState(inputMap.actions[i]);
            }
        }
        
        private void UpdateInputMapsBlockState(NativeList<Guid> guids) {
            for (int i = 0; i < guids.Length; i++) {
                UpdateInputMapBlockState(_inputMapper.GetInputMap(guids[i]));
            }
        }

        private void UpdateInputActionBlockState(InputAction inputAction) {
            if (inputAction == null) return;
            
            var guid = inputAction.id;
            int count = _inputActionToSourceHashBlocks.GetCount(guid);
            
            bool hasOverride = false;
            bool enableInputAction = false;
            
            for (int i = 0; i < count; i++) {
                int hash = _inputActionToSourceHashBlocks.GetValueAt(guid, i);
                if (!_inputActionBlocks.TryGetValue(new BlockKey(hash, guid), out var block)) continue;
                
                hasOverride = true;
                
                if (block.blocked) continue;

                enableInputAction = true;
                break;
            }
            
            if (!hasOverride) {
                enableInputAction = inputAction.actionMap == null || 
                                    !_inputMapToSourceHashBlocks.ContainsKey(inputAction.actionMap.id);
            }
            
            if (enableInputAction) inputAction.Enable();
            else inputAction.Disable();
        }
     
        private void UpdateInputActionsBlockState(NativeList<Guid> guids) {
            for (int i = 0; i < guids.Length; i++) {
                UpdateInputActionBlockState(_inputMapper.GetInputAction(guids[i]));
            }
        }

        private static void LogInfo(string message) {
            Debug.Log($"{nameof(InputBlockService).FormatColorOnlyForEditor(Color.white)}: f {Time.frameCount}, {message}");
        }

        private string GetInputsStateString() {
            var sb = new StringBuilder();

            for (int i = 0; i < _inputMapper.InputMaps.Count; i++) {
                var inputMap = _inputMapper.InputMaps[i];
                sb.AppendLine($"{inputMap.name} {FormatStateString(IsInputMapEnabled(inputMap))} {FormatMapBlocksString(inputMap.id)}");

                var inputActions = inputMap.actions;

                for (int j = 0; j < inputActions.Count; j++) {
                    var inputAction = inputActions[j];
                    sb.AppendLine($"- {inputAction.name} {FormatStateString(IsInputActionEnabled(inputAction))} {FormatActionBlocksString(inputAction.id)}");
                }
            }

            return sb.ToString();
        }

        private string JoinActionsToString(IReadOnlyList<InputAction> inputActions) {
            var sb = new StringBuilder();

            for (int i = 0; i < inputActions?.Count; i++) {
                var inputAction = inputActions[i];
                if (inputAction == null) continue;

                sb.Append($"{(inputAction.actionMap == null ? null : $"{inputAction.actionMap.name}/")}{inputAction.name}{(i < inputActions.Count - 1 ? ", " : null)}");
            }
            
            return sb.ToString();
        }
        
        private string JoinActionsToString(IReadOnlyList<InputActionRef> inputActionRefs) {
            var sb = new StringBuilder();

            for (int i = 0; i < inputActionRefs?.Count; i++) {
                var inputAction = _inputMapper.GetInputAction(inputActionRefs[i].Guid);
                if (inputAction == null) continue;

                sb.Append($"{(inputAction.actionMap == null ? null : $"{inputAction.actionMap.name}/")}{inputAction.name}{(i < inputActionRefs.Count - 1 ? ", " : null)}");
            }
            
            return sb.ToString();
        }
        
        private string JoinActionsToString(NativeHashSet<Guid> inputActions) {
            var sb = new StringBuilder();
            int count = inputActions.Count;
            int index = 0;
            
            foreach (var guid in inputActions) {
                index++;
                
                var inputAction = _inputMapper.GetInputAction(guid);
                if (inputAction == null) continue;

                sb.Append($"{(inputAction.actionMap == null ? null : $"{inputAction.actionMap.name}/")}{inputAction.name}{(index < count ? ", " : null)}");
            }
            
            return sb.ToString();
        }
        
        private string JoinMapsToString(IReadOnlyList<InputActionMap> inputMaps) {
            var sb = new StringBuilder();

            for (int i = 0; i < inputMaps?.Count; i++) {
                var inputMap = inputMaps[i];
                if (inputMap == null) continue;

                sb.Append($"{inputMap.name}{(i < inputMaps.Count - 1 ? ", " : null)}");
            }
            
            return sb.ToString();
        }

        private string JoinMapsToString(IReadOnlyList<InputMapRef> inputMapRefs) {
            var sb = new StringBuilder();

            for (int i = 0; i < inputMapRefs?.Count; i++) {
                var inputMap = _inputMapper.GetInputMap(inputMapRefs[i].Guid);
                if (inputMap == null) continue;

                sb.Append($"{inputMap.name}{(i < inputMapRefs.Count - 1 ? ", " : null)}");
            }
            
            return sb.ToString();
        }
        
        private string JoinMapsToString(NativeHashSet<Guid> inputMaps) {
            var sb = new StringBuilder();
            int count = inputMaps.Count;
            int index = 0;
            
            foreach (var guid in inputMaps) {
                index++;
                
                var inputMap = _inputMapper.GetInputMap(guid);
                if (inputMap == null) continue;

                sb.Append($"{inputMap.name}{(index < count ? ", " : null)}");
            }
            
            return sb.ToString();
        }
        
        private string FormatMapBlocksString(Guid guid) {
            int count = _inputMapToSourceHashBlocks.GetCount(guid);
            return count > 0 ? $"({count} blocks)" : null;
        }
        
        private string FormatActionBlocksString(Guid guid) {
            int count = _inputActionToSourceHashBlocks.GetCount(guid);
            return count > 0 ? $"({count} block overrides)" : null;
        }
        
        private static string FormatStateString(bool enableState) {
            return enableState 
                ? "ON".FormatColorOnlyForEditor(Color.green) 
                : "OFF".FormatColorOnlyForEditor(Color.red);
        }
    }
    
}