using System;
using System.Collections.Generic;
using MisterGames.Common.Data;
using MisterGames.Common.Lists;
using UnityEngine;

namespace MisterGames.Blueprints.Core {
    
    [Serializable]
    public struct Link {
        
        public BlueprintNode remote;
        public int remotePort;

        public override string ToString() {
            return $"{remotePort}::{remote}";
        }
    }
    
    [Serializable]
    public struct Port {

        public string Name => FormatName();
        public Color Color => _color;
        
        public bool IsExit => _isExit;
        public bool IsExposed => _isExposed;
        public bool IsMultiple => _isMultiple;
        
        public Link[] Links => _links.ToArray();

        internal bool IsOwned => _isOwned;
        internal bool IsBuiltIn => _isBuiltIn;
        internal int OwnerPort => _ownerPort;
        internal string RawName => _name;
        
        [SerializeField] private string _name;
        [SerializeField] private string _nameColor;
        [SerializeField] private Color _color;
        [SerializeField] private bool _isBuiltIn;
        [SerializeField] private bool _isViewCopy;
        
        [SerializeField] private bool _isExit;
        [SerializeField] private bool _isMultiple;
        [SerializeField] private bool _isExposed;
        [SerializeField] private bool _isOwned;
        [SerializeField] private bool _isDataPort;
        [SerializeField] private bool _hasType;
        [SerializeField] private string _serializedType;
        
        [SerializeField] private int _ownerPort;
        [SerializeField] private List<Link> _links;

        public static Port Enter(string name = "") {
            return new Port {
                _name = name ?? "",
                _nameColor = BlueprintColors.Port.Names.Flow,
                _isDataPort = false,
                _isExit = false,
                _isOwned = false,
                _isMultiple = true,
                _color = BlueprintColors.Port.Links.Flow,
                _links = new List<Link>()
            };
        }

        public static Port Exit(string name = "") {
            return new Port {
                _name = name ?? "",
                _nameColor = BlueprintColors.Port.Names.Flow,
                _isDataPort = false,
                _isExit = true,
                _isOwned = true,
                _isMultiple = true,
                _color = BlueprintColors.Port.Links.Flow,
                _links = new List<Link>()
            };
        }

        public static Port Input<T>(string name = "") {
            return new Port {
                _name = name ?? "",
                _nameColor = BlueprintColors.Port.Names.GetColorForType<T>(),
                _serializedType = SerializedType.ToString(typeof(T)),
                _hasType = true,
                _isDataPort = true,
                _isExit = false,
                _isOwned = true,
                _isMultiple = false,
                _color = BlueprintColors.Port.Links.GetColorForType<T>(),
                _links = new List<Link>()
            };
        }

        public static Port Output<T>(string name = "") {
            return new Port {
                _name = name ?? "",
                _nameColor = BlueprintColors.Port.Names.GetColorForType<T>(),
                _serializedType = SerializedType.ToString(typeof(T)),
                _hasType = true,
                _isDataPort = true,
                _isExit = true,
                _isOwned = false,
                _isMultiple = true,
                _color = BlueprintColors.Port.Links.GetColorForType<T>(),
                _links = new List<Link>()
            };
        }

        internal static Port Input(string name = "") {
            return new Port {
                _name = name ?? "",
                _nameColor = BlueprintColors.Port.Names.Data,
                _isDataPort = true,
                _isExit = false,
                _isOwned = true,
                _isMultiple = false,
                _color = BlueprintColors.Port.Links.Data,
                _links = new List<Link>()
            };
        }

        internal static Port Output(string name = "") {
            return new Port {
                _name = name ?? "",
                _nameColor = BlueprintColors.Port.Names.Data,
                _isDataPort = true,
                _isExit = true,
                _isOwned = false,
                _isMultiple = true,
                _color = BlueprintColors.Port.Links.Data,
                _links = new List<Link>()
            };
        }
        
        public Port Colored(Color color) {
            _color = color;
            return this;
        }
        
        public bool IsCompatibleWith(BlueprintNode owner, in Port remotePort, BlueprintNode remote) {
            if (remotePort._isExit == _isExit) return false;
            if (remotePort._isDataPort != _isDataPort) return false;
            if (!remotePort._isDataPort && !_isDataPort) return true;
            if (!remotePort._hasType || !_hasType) return true;

            if (!remotePort._isExit && SerializedType.FromString(remotePort._serializedType) == typeof(string)) {
                return owner is IBlueprintGetter<string>;
            }

            if (!_isExit && SerializedType.FromString(_serializedType) == typeof(string)) {
                return remote is IBlueprintGetter<string>;
            }
            
            return remotePort._serializedType == _serializedType || _isViewCopy;
        }

        internal void Dispose() {
            _links.Clear();
        }

        internal void ConnectTo(BlueprintNode owner, int port, BlueprintNode remote, int remotePort, bool ignoreOwnership) {
            if (!ignoreOwnership && !_isOwned || HasConnectedPortTo(remote, remotePort)) { 
                return;
            }

            if (!_isMultiple && _links.Count > 0) {
                DisconnectLink(_links[0], owner, port);
                _links.Clear();
            }
            
            _links.Add(new Link { remotePort = remotePort, remote = remote });
        }

        internal void DisconnectFrom(BlueprintNode remote, int remotePort) {
            if (!_isOwned) return;
            
            for (int i = 0; i < _links.Count; i++) {
                var link = _links[i];
                if (link.remotePort == remotePort && link.remote.Guid == remote.Guid) {
                    _links.RemoveAt(i);
                    return;
                }
            }
        }

        internal void DisconnectAll(BlueprintNode owner, int port) {
            if (!_isOwned) return;
            
            for (int i = 0; i < _links.Count; i++) {
                DisconnectLink(_links[i], owner, port);
            }

            _links.Clear();
        }

        private void DisconnectLink(in Link link, BlueprintNode owner, int port) {
            link.remote.AsIBlueprintNode().DisconnectPort(link.remotePort, owner, port);
        }

        internal void DisconnectAllLinkedWith(BlueprintNode remote) {
            if (!_isOwned) return;
            
            for (int i = 0; i < _links.Count; i++) {
                var link = _links[i];
                if (link.remote.Guid == remote.Guid) {
                    _links.RemoveAt(i--);
                }
            }
        }

        internal void ResolveLinks(IBlueprintResolver resolver) {
            for (int i = 0; i < _links.Count; i++) {
                var link = _links[i];
                link.remote = resolver.Resolve(link.remote);
                _links[i] = link;
            }
        }

        internal T Read<T>(BlueprintNode owner, int port, T defaultValue = default) {
            return _links.Count == 0 ? defaultValue : ReadLink(owner, port, _links[0], defaultValue);
        }

        internal bool TryCallLink(BlueprintNode owner, int port, in Link link) {
            var remote = link.remote;
            int remotePort = link.remotePort;

            if (!remote.HasPort(remotePort)) {
                Debug.LogError($"Blueprint {owner.m_NodeOwner.name}, node {owner.name} from flow port {port} " +
                               $"is trying to call not existing remote flow port {remotePort} on remote node {remote.name}.");
                return false;
            }

            var connectedPort = remote.AsIBlueprintNode().Ports[remotePort];
            if (connectedPort._isDataPort) {
                Debug.LogError($"Blueprint {owner.m_NodeOwner.name}, node {owner.name} from <b>flow</b> port {port} " +
                               $"is trying to call remote <b>data</b> port {remotePort} on remote node {remote.name}.");
                return false;
            }

            if (!(remote is IBlueprintEnter flow)) {
                Debug.LogError($"Blueprint {owner.m_NodeOwner.name}, node {owner.name} from flow port {port} " +
                               $"is trying to call remote flow port {remotePort} on remote node {remote.name}, " +
                               $"but remote node {remote.name} does not implement <b>IEnter</b> interface.");
                return false;
            }
            
            flow.Enter(remotePort);
            return true;
        }
        
        private T ReadLink<T>(BlueprintNode owner, int port, in Link link, T defaultValue) {
            var remote = link.remote;
            int remotePort = link.remotePort;

            if (!remote.HasPort(remotePort)) {
                Debug.LogError($"Blueprint {owner.m_NodeOwner.name}, node {owner.name} from data port {port} " +
                               $"is trying to read not existing remote data port {remotePort} on remote node {remote.name}.");
                return defaultValue;
            }

            var connectedPort = remote.AsIBlueprintNode().Ports[remotePort];
            if (!connectedPort._isDataPort) {
                Debug.LogError($"Blueprint {owner.m_NodeOwner.name}, node {owner.name} from <b>data</b> port {port} " +
                               $"is trying to read remote <b>flow</b> port {remotePort} on remote node {remote.name}.");
                return defaultValue;
            }
            
            if (remote is IBlueprintGetter<T> getterT) {
                return getterT.Get(remotePort);
            }

            if (remote is IBlueprintGetter getter) {
                return getter.Get<T>(remotePort);
            }
            
            Debug.LogError($"Blueprint {owner.m_NodeOwner.name}, node {owner.name} from data port {port} " +
                           $"is trying to read remote data port {remotePort} on remote node {remote.name}, " +
                           $"but remote node {remote.name} does not implement <b>IGetter<T></b> interface.");
            
            return defaultValue;
        }

        private bool HasConnectedPortTo(BlueprintNode remote, int remotePort) {
            for (int i = 0; i < _links.Count; i++) {
                var link = _links[i];
                if (link.remotePort == remotePort && link.remote.Guid == remote.Guid) {
                    return true;
                }
            }
            return false;
        }

        internal Port Exposed() {
            _isExposed = true;
            return this;
        }
        
        internal Port NotExposed() {
            _isExposed = false;
            return this;
        }
        
        internal Port CopyViewFrom(in Port other) {
            _serializedType = other._serializedType;
            _hasType = other._hasType;
            _color = other._color;
            _nameColor = other._nameColor;
            _isViewCopy = true;
            return this;
        }

        internal Port CopyWithoutLinks() {
            return new Port { 
                _name = _name,
                _nameColor = _nameColor,
                _color = _color,
                _isBuiltIn = _isBuiltIn,
                _isViewCopy = _isViewCopy,
                _isExit = _isExit,
                _isMultiple = _isMultiple,
                _isExposed = _isExposed,
                _isOwned = _isOwned,
                _isDataPort = _isDataPort,
                _hasType = _hasType,
                _serializedType = _serializedType,
                _ownerPort = _ownerPort,
                _links = new List<Link>()
            };
        }
        
        internal Port CopyLinksFrom(in Port other) {
            _ownerPort = other._ownerPort;
            _links = new List<Link>();

            var otherLinks = other._links;
            for (int i = 0; i < otherLinks.Count; i++) {
                var link = otherLinks[i];
                
                var remote = link.remote;
                if (remote == null) continue;
                
                if (remote.HasPort(link.remotePort)) _links.Add(link);
            }
            
            return this;
        }

        internal bool HasSameSignature(in Port other) {
            return _isOwned == other._isOwned &&
                   _isExit == other._isExit &&
                   _isMultiple == other._isMultiple &&
                   _isExposed == other._isExposed &&
                   _isDataPort == other._isDataPort &&
                   _hasType == other._hasType &&
                   (!_hasType || _serializedType == other._serializedType);
        }
        
        internal void WithOwnerPort(int port) {
            _ownerPort = port;
        }
        
        internal Port BuiltIn() {
            _isBuiltIn = true;
            return this;
        }

        private string FormatName() {
            string format = $"<color={_nameColor}>{_name.Trim()}</color>";
            return _isBuiltIn ? $"<b>{format}</b>" : format;
        }
        
        public override string ToString() {
            return $"Port({_name} [" +
                   (_isDataPort ? _isExit ? "output" : "input" : _isExit ? "exit" : "enter") +
                   (_hasType ? $"] of type [{SerializedType.FromString(_serializedType)}]" : "]") +
                   $": [{string.Join(", ", _links)}]" +
                   ")";
        }
    }

}