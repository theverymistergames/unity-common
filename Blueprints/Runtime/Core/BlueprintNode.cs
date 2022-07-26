using System;
using System.Collections.Generic;
using MisterGames.Common.Data;
using UnityEngine;

namespace MisterGames.Blueprints.Core {
 
    public interface IBlueprintNode {

        Vector2 Position { get; set; }
        
        event Action OnPortsUpdated;
        Port[] Ports { get; }

        void InitPorts();
        void DeInitPorts();
        
        void ConnectPort(int port, BlueprintNode remote, int remotePort);
        void DisconnectPort(int port, BlueprintNode remote, int remotePort);
        void DisconnectAllPorts();

    }
    
    public abstract class BlueprintNode : ScriptableObjectWithId, IBlueprintNode {

        [HideInInspector] 
        [SerializeField] private Vector2 m_NodePosition;
        Vector2 IBlueprintNode.Position {
            get => m_NodePosition;
            set => m_NodePosition = value;
        }

        [HideInInspector] 
        [SerializeField] private List<Port> m_NodePorts = new List<Port>();
        Port[] IBlueprintNode.Ports => m_NodePorts.ToArray();

        [HideInInspector] 
        [SerializeField] internal Blueprint m_NodeOwner;
         
        private event Action _onPortsUpdated = delegate {  };
        event Action IBlueprintNode.OnPortsUpdated {
            add => _onPortsUpdated += value;
            remove => _onPortsUpdated -= value;
        }

        public int FlowCount { get; internal set; } = 0;
        internal virtual bool HasStablePorts => true;
        internal virtual bool HasExposedPorts => false;

        protected BlueprintRunner runner;
        protected Blackboard blackboard;
        internal IBlueprintHost host;

        protected virtual void OnInit() {}
        protected virtual void OnTerminate() {}
        internal virtual void OnStart() {}

        internal virtual void OnResolveLinks(BlueprintNode original, IBlueprintResolver resolver) {}

        protected abstract IReadOnlyList<Port> CreatePorts();
        internal virtual void OnInitPorts() {}
        internal virtual void OnDeInitPorts() {}
        
        protected virtual void OnPortConnected(BlueprintNode node, int port) {}
        protected virtual void OnPortDisconnected(BlueprintNode node, int port) {}

        internal void Init(BlueprintRunner runner, IBlueprintHost host) {
            this.runner = runner;
            this.host = host;
            blackboard = host.Instance.Blackboard;
            OnInit();
        }
        
        internal void Terminate() {
            OnTerminate();
        }

        internal void ResolveLinks(BlueprintNode original, IBlueprintResolver resolver) {
            for (int i = 0; i < m_NodePorts.Count; i++) {
                var port = m_NodePorts[i];
                port.ResolveLinks(resolver);
                m_NodePorts[i] = port;
            }
            OnResolveLinks(original, resolver);
        }
        
        internal bool HasPort(int port) {
            return 0 <= port && port < this.AsIBlueprintNode().Ports.Length;
        }
        
        void IBlueprintNode.InitPorts() {
            OnInitPorts();
            InvalidatePorts();
        }

        void IBlueprintNode.DeInitPorts() {
            OnDeInitPorts();
        }

        void IBlueprintNode.ConnectPort(int port, BlueprintNode remote, int remotePort) {
            ConnectPort(port, remote, remotePort, false);
        }

        void IBlueprintNode.DisconnectPort(int port, BlueprintNode remote, int remotePort) {
            if (HasPort(port)) {
                var p = m_NodePorts[port];
                p.DisconnectFrom(remote, remotePort);
                m_NodePorts[port] = p;
                
                OnPortDisconnected(remote, port);
            }
        }

        void IBlueprintNode.DisconnectAllPorts() {
            for (int i = 0; i < m_NodePorts.Count; i++) {
                var p = m_NodePorts[i];
                p.DisconnectAll(this, i);
                m_NodePorts[i] = p;
            }
        }

        internal void ConnectPort(int port, BlueprintNode remote, int remotePort, bool ignoreOwnership) {
            if (HasPort(port)) {
                var p = m_NodePorts[port];
                p.ConnectTo(this, port, remote, remotePort, ignoreOwnership);
                m_NodePorts[port] = p;
                
                OnPortConnected(remote, port);
            }
        }

        internal void SetOwnerPort(int port, int ownerPort) {
            if (HasPort(port)) {
                var p = m_NodePorts[port];
                p.WithOwnerPort(ownerPort);
                m_NodePorts[port] = p;
            }
        }
        
        protected void InvalidatePorts() {
            if (m_NodeOwner == null) return;
            
            var prevPorts = new Port[m_NodePorts.Count];
            m_NodePorts.CopyTo(prevPorts);

            m_NodePorts.Clear( );
            m_NodePorts.AddRange(CreatePorts());
            
            if (HasStablePorts) {
                for (int p = 0; p < m_NodePorts.Count; p++) {
                    var port = m_NodePorts[p];
                    if (p >= prevPorts.Length) continue;
                    
                    var prevPort = prevPorts[p];
                    if (port.HasSameSignature(prevPort)) { 
                        m_NodePorts[p] = port.CopyLinksFrom(prevPort);    
                    }
                }
            }
            else {
                var linkedPorts = new List<int>();
                
                for (int p = 0; p < m_NodePorts.Count; p++) {
                    var port = m_NodePorts[p];
                    string portName = port.RawName;
                    
                    for (int i = 0; i < prevPorts.Length; i++) {
                        var prevPort = prevPorts[i];
                        string prevPortName = prevPort.RawName;
                        
                        if (portName == prevPortName && port.HasSameSignature(prevPort)) {
                            m_NodePorts[p] = port.CopyLinksFrom(prevPort);
                            linkedPorts.Add(i);
                            break;
                        }
                    }
                }

                for (int p = 0; p < prevPorts.Length; p++) {
                    if (linkedPorts.Contains(p)) continue;
                    prevPorts[p].DisconnectAll(this, p);
                }
            }

            _onPortsUpdated.Invoke();
        }
        
        protected void Call(int port) {
            if (!HasPort(port)) {
                Debug.LogError($"Blueprint {m_NodeOwner.name}, node {name} is trying " +
                               $"to call not existing flow port {port} on itself.");
                return;
            }

            var p = m_NodePorts[port];
            var links = p.Links;

            for (int i = 0; i < links.Length; i++) {
                var link = links[i];
                if (p.TryCallLink(this, port, link)) {
                    host.OnCalled(this, link.remote);
                }
            }
        }

        protected T Read<T>(int port, T defaultValue = default) {
            if (!HasPort(port)) {
                Debug.LogError($"Blueprint {m_NodeOwner.name}, node {name} is trying " +
                               $"to read not existing data port {port} on itself.");
                return defaultValue;
            }
            
            return m_NodePorts[port].Read(this, port, defaultValue);
        }

        public override string ToString() {
            return name;
        }
    }

}