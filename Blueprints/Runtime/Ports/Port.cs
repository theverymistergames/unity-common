using System;
using System.Collections.Generic;
using UnityEngine;

namespace MisterGames.Blueprints.Ports {

    [Serializable]
    public struct Port {

        [SerializeField] [HideInInspector] private PortMeta _portMeta;
        public PortMeta Meta => _portMeta;

        [SerializeField] [HideInInspector] private List<Link> _links;
        public IReadOnlyList<Link> Links => _links;

        public void AddLink(int nodeId, int port) {
            _links.Add(new Link { nodeId = nodeId, port = port });
        }

        public void ClearLinks() {
            _links.Clear();
        }

        public override string ToString() {
            return $"Port(links: {string.Join(", ", _links)})";
        }

        public static Port Enter(string name = "") {
            return new Port {
                _portMeta = new PortMeta {
                    name = name,
                    isDataPort = false,
                    isExitPort = false,
                    hasDataType = false,
                },
                _links = new List<Link>(),
            };
        }

        public static Port Exit(string name = "") {
            return new Port {
                _portMeta = new PortMeta {
                    name = name,
                    isDataPort = false,
                    isExitPort = true,
                    hasDataType = false,
                },
                _links = new List<Link>(),
            };
        }

        public static Port Input<T>(string name = "") {
            return new Port {
                _portMeta = new PortMeta {
                    name = name,
                    isDataPort = true,
                    isExitPort = false,
                    hasDataType = false,
                    dataType = typeof(T),
                },
                _links = new List<Link>(),
            };
        }

        public static Port Output<T>(string name = "") {
            return new Port {
                _portMeta = new PortMeta {
                    name = name,
                    isDataPort = true,
                    isExitPort = true,
                    hasDataType = true,
                    dataType = typeof(T),
                },
                _links = new List<Link>(),
            };
        }

        internal static Port Input(string name = "") {
            return new Port {
                _portMeta = new PortMeta {
                    name = name,
                    isDataPort = true,
                    isExitPort = false,
                    hasDataType = false,
                },
                _links = new List<Link>(),
            };
        }

        internal static Port Output(string name = "") {
            return new Port {
                _portMeta = new PortMeta {
                    name = name,
                    isDataPort = true,
                    isExitPort = true,
                    hasDataType = false,
                },
                _links = new List<Link>(),
            };
        }
    }

}
