using System;
using System.Collections.Generic;
using System.Threading;
using MisterGames.Common.Data;
using Unity.Collections;
using UnityEngine.InputSystem;

namespace MisterGames.Input.Core {
    
    public sealed class InputService : IDisposable {

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
        
        private readonly Dictionary<Guid, InputAction> _inputActions = new();
        private readonly Dictionary<Guid, InputActionMap> _inputMaps = new();

        private readonly MultiValueDictionary<int, Guid> _sourceHashToInputActionBlocks = new();
        private readonly MultiValueDictionary<Guid, int> _inputActionToSourceHashBlocks = new();
        private readonly MultiValueDictionary<int, Guid> _sourceHashToInputMapBlocks = new();
        private readonly MultiValueDictionary<Guid, int> _inputMapToSourceHashBlocks = new();
        private readonly Dictionary<BlockKey, BlockData> _blockMap = new();
        
        public void Dispose() {
            _inputActions.Clear();
            _inputMaps.Clear();
        }

        public InputAction GetInputAction(InputActionRef inputActionRef) {
            return GetInputAction(inputActionRef.guid.ToGuid());
        }
        
        public InputActionMap GetInputMap(InputMapRef inputMapRef) {
            return GetInputMap(inputMapRef.guid.ToGuid());
        }
        
        public InputAction GetInputAction(Guid guid) {
            if (!_inputActions.TryGetValue(guid, out var action)) {
                action = InputSystem.actions.FindAction(guid);
                _inputActions[guid] = action;
            }

            return action;
        }
        
        public InputActionMap GetInputMap(Guid guid) {
            if (!_inputMaps.TryGetValue(guid, out var map)) {
                map = InputSystem.actions.FindActionMap(guid);
                _inputMaps[guid] = map;
            }

            return map;
        }

        public bool IsInputActionEnabled(InputAction inputAction) {
            return inputAction is { enabled: true } && IsInputMapEnabled(inputAction.actionMap);
        }
        
        public bool IsInputMapEnabled(InputActionMap inputMap) {
            return inputMap is { enabled: true } && !_inputMapToSourceHashBlocks.ContainsKey(inputMap.id);
        }

        #region InputActionBlockOverrides
        
        public bool SetInputActionBlockOverride(object source, InputAction inputAction, bool blocked, CancellationToken cancellationToken = default) {
            if (SetInputActionBlockOverrideInternal(source, inputAction, blocked, cancellationToken)) {
                UpdateInputActionBlockState(inputAction.id);
                return true;
            }
            
            return false;
        }

        public bool SetInputActionBlockOverride(object source, InputActionRef inputActionRef, bool blocked, CancellationToken cancellationToken = default) {
            var inputAction = GetInputAction(inputActionRef);
            
            if (SetInputActionBlockOverrideInternal(source, inputAction, blocked, cancellationToken)) {
                UpdateInputActionBlockState(inputAction.id);
                return true;
            }
            
            return false;
        }

        public bool SetInputActionBlockOverrides(object source, IReadOnlyList<InputAction> inputActions, bool blocked, CancellationToken cancellationToken = default) {
            var changedList = new NativeList<Guid>(Allocator.Temp);
            
            for (int i = 0; i < inputActions?.Count; i++) {
                var inputAction = inputActions[i];
                
                if (SetInputActionBlockOverrideInternal(source, inputAction, blocked, cancellationToken)) {
                    changedList.Add(inputAction.id);
                }
            }

            int count = changedList.Length;
            if (count > 0) UpdateInputActionsBlockState(changedList);
            
            changedList.Dispose();
            
            return count > 0;
        }

        public bool SetInputActionBlockOverrides(object source, IReadOnlyList<InputActionRef> inputActionRefs, bool blocked, CancellationToken cancellationToken = default) {
            var changedList = new NativeList<Guid>(Allocator.Temp);
            
            for (int i = 0; i < inputActionRefs?.Count; i++) {
                var inputAction = GetInputAction(inputActionRefs[i]);
                
                if (SetInputActionBlockOverrideInternal(source, inputAction, blocked, cancellationToken)) {
                    changedList.Add(inputAction.id);
                }
            }

            int count = changedList.Length;
            if (count > 0) UpdateInputActionsBlockState(changedList);
            
            changedList.Dispose();
            
            return count > 0;
        }

        public bool RemoveInputActionBlockOverride(object source, InputAction inputAction) {
            if (RemoveInputActionBlockOverrideInternal(source, inputAction)) {
                UpdateInputActionBlockState(inputAction.id);
                return true;
            }
            
            return false;
        }
        
        public bool RemoveInputActionBlockOverride(object source, InputActionRef inputActionRef) {
            var inputAction = GetInputAction(inputActionRef);
            
            if (RemoveInputActionBlockOverrideInternal(source, inputAction)) {
                UpdateInputActionBlockState(inputAction.id);
                return true;
            }
            
            return false;
        }

        public bool RemoveInputActionBlockOverrides(object source, IReadOnlyList<InputAction> inputActions) {
            var changedList = new NativeList<Guid>(Allocator.Temp);
            
            for (int i = 0; i < inputActions?.Count; i++) {
                var inputAction = inputActions[i];
                
                if (RemoveInputActionBlockOverrideInternal(source, inputAction)) {
                    changedList.Add(inputAction.id);
                }
            }

            int count = changedList.Length;
            if (count > 0) UpdateInputActionsBlockState(changedList);
            
            changedList.Dispose();
            
            return count > 0;
        }
        
        public bool RemoveInputActionBlockOverrides(object source, IReadOnlyList<InputActionRef> inputActionRefs) {
            var changedList = new NativeList<Guid>(Allocator.Temp);
            
            for (int i = 0; i < inputActionRefs?.Count; i++) {
                var inputAction = GetInputAction(inputActionRefs[i]);
                
                if (RemoveInputActionBlockOverrideInternal(source, inputAction)) {
                    changedList.Add(inputAction.id);
                }
            }

            int count = changedList.Length;
            if (count > 0) UpdateInputActionsBlockState(changedList);
            
            changedList.Dispose();
            
            return count > 0;
        }
        
        public bool ClearInputActionBlockOverridesFor(object source) {
            int hash = source.GetHashCode();
            int count = _sourceHashToInputActionBlocks.GetCount(hash);

            if (count <= 0) return false;
            
            var changedList = new NativeList<Guid>(Allocator.Temp);
            
            for (int i = count - 1; i >= 0; i--) {
                if (!_sourceHashToInputActionBlocks.RemoveValueAt(hash, i, out var guid)) continue;
                
                _inputActionToSourceHashBlocks.RemoveValue(guid, hash);
                    
                if (_blockMap.Remove(new BlockKey(hash, guid))) changedList.Add(guid);
            }
            
            int changedCount = changedList.Length;
            if (changedCount > 0) UpdateInputActionsBlockState(changedList);
            
            changedList.Dispose();
            
            return changedCount > 0;
        }
        
        public bool ClearAllInputActionBlockOverrides() {
            var changedList = new NativeList<Guid>(Allocator.Temp);

            foreach (int hash in _sourceHashToInputActionBlocks.Keys) {
                int count = _sourceHashToInputActionBlocks.GetCount(hash);
                
                for (int i = count - 1; i >= 0; i--) {
                    if (!_sourceHashToInputActionBlocks.TryGetValueAt(hash, i, out var guid)) continue;
                    
                    if (_blockMap.Remove(new BlockKey(hash, guid))) changedList.Add(guid);
                }
            }
            
            _sourceHashToInputActionBlocks.Clear();
            _inputActionToSourceHashBlocks.Clear();
            
            int changedCount = changedList.Length;
            if (changedCount > 0) UpdateInputActionsBlockState(changedList);
            
            changedList.Dispose();
            
            return changedCount > 0;
        }
        
        private bool SetInputActionBlockOverrideInternal(object source, InputAction inputAction, bool blocked, CancellationToken cancellationToken) {
            if (inputAction == null) return false;
            
            var guid = inputAction.id;
            int hash = source.GetHashCode();

            if (cancellationToken.IsCancellationRequested) {
                _sourceHashToInputActionBlocks.RemoveValue(hash, guid);
                _inputActionToSourceHashBlocks.RemoveValue(guid, hash);
                
                return _blockMap.Remove(new BlockKey(hash, guid));
            }

            if (!_sourceHashToInputActionBlocks.ContainsValue(hash, guid)) {
                _sourceHashToInputActionBlocks.AddValue(hash, guid);
            }
            
            if (!_inputActionToSourceHashBlocks.ContainsValue(guid, hash)) {
                _inputActionToSourceHashBlocks.AddValue(guid, hash);
            }
            
            _blockMap[new BlockKey(hash, guid)] = new BlockData(blocked, cancellationToken);
            return true;
        }
        
        private bool RemoveInputActionBlockOverrideInternal(object source, InputAction inputAction) {
            if (inputAction == null) return false;
            
            var guid = inputAction.id;
            int hash = source.GetHashCode();

            _sourceHashToInputActionBlocks.RemoveValue(hash, guid);
            _inputActionToSourceHashBlocks.RemoveValue(guid, hash);
            
            return _blockMap.Remove(new BlockKey(hash, guid));
        }
        
        private void UpdateInputActionBlockState(Guid guid) {
            var inputAction = GetInputAction(guid);
            if (inputAction == null) return;
            
            int count = _inputActionToSourceHashBlocks.GetCount(guid);
            bool hasOverride = false;
            bool enableInputAction = false;

            for (int i = 0; i < count; i++) {
                int hash = _inputActionToSourceHashBlocks.GetValueAt(guid, i);
                if (!_blockMap.TryGetValue(new BlockKey(hash, guid), out var block)) continue;

                hasOverride = true;
                
                if (block.blocked) continue;

                enableInputAction = true;
                break;
            }

            if (!hasOverride) {
                enableInputAction = inputAction.actionMap == null || 
                                    inputAction.actionMap.enabled && !_inputMapToSourceHashBlocks.ContainsKey(inputAction.actionMap.id);
            }

            if (enableInputAction) inputAction.Enable();
            else inputAction.Disable();
        }
     
        private void UpdateInputActionsBlockState(NativeList<Guid> guids) {
            for (int i = 0; i < guids.Length; i++) {
                UpdateInputActionBlockState(guids[i]);
            }
        }
        
        #endregion
    }
    
}