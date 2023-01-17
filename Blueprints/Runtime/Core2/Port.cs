﻿using System;
using MisterGames.Common.Data;
using UnityEngine;

namespace MisterGames.Blueprints.Core2 {

    [Serializable]
    public struct Port : IEquatable<Port> {

        public string name;
        public bool isDataPort;
        public bool isExitPort;
        public bool isExternalPort;
        public bool hasDataType;
        public int dataTypeHash;

        [SerializeField] private string _serializedDataType;

        public Type DataType {
            get => SerializedType.FromString(_serializedDataType);
            set {
                dataTypeHash = value.GetHashCode();
                _serializedDataType = SerializedType.ToString(value);
            }
        }

        public int GetSignatureHashCode() {
            return HashCode.Combine(isDataPort, isExitPort, isExitPort, hasDataType, dataTypeHash, _serializedDataType);
        }

        public bool Equals(Port other) {
            return name == other.name &&
                   isDataPort == other.isDataPort &&
                   isExitPort == other.isExitPort &&
                   isExternalPort == other.isExternalPort &&
                   hasDataType == other.hasDataType &&
                   dataTypeHash == other.dataTypeHash &&
                   _serializedDataType == other._serializedDataType;
        }

        public override bool Equals(object obj) {
            return obj is Port other && Equals(other);
        }

        public override int GetHashCode() {
            return HashCode.Combine(name, isDataPort, isExitPort, isExternalPort, hasDataType, dataTypeHash, _serializedDataType);
        }

        public static bool operator ==(Port left, Port right) {
            return left.Equals(right);
        }

        public static bool operator !=(Port left, Port right) {
            return !left.Equals(right);
        }

        public override string ToString() {
            string mode = isDataPort
                ? $"{(isExitPort ? "output" : "input")}{(hasDataType ? $"<{_serializedDataType}>" : "")}"
                : isExitPort
                    ? "exit"
                    : "enter";

            return $"Port({name}, mode {mode})";
        }

        public static Port Enter(string name = "") {
            return new Port {
                name = name,
                isDataPort = false,
                isExitPort = false,
                hasDataType = false,
            };
        }

        public static Port Exit(string name = "") {
            return new Port {
                name = name,
                isDataPort = false,
                isExitPort = true,
                hasDataType = false,
            };
        }

        public static Port Input<T>(string name = "") {
            return new Port {
                name = name,
                isDataPort = true,
                isExitPort = false,
                hasDataType = true,
                DataType = typeof(T),
            };
        }

        public static Port Output<T>(string name = "") {
            return new Port {
                name = name,
                isDataPort = true,
                isExitPort = true,
                hasDataType = true,
                DataType = typeof(T),
            };
        }

        internal static Port Input(string name = "") {
            return new Port {
                name = name,
                isDataPort = true,
                isExitPort = false,
                hasDataType = false,
            };
        }

        internal static Port Output(string name = "") {
            return new Port {
                name = name,
                isDataPort = true,
                isExitPort = true,
                hasDataType = false,
            };
        }

        internal Port SetExternal(bool isExternal) {
            isExternalPort = isExternal;
            return this;
        }
    }
}