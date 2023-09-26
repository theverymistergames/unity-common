using System;
using System.Collections.Generic;
using MisterGames.Common.Types;
using NUnit.Framework;
using UnityEngine;

namespace Data {

    public class SerializedTypeTests {

        [Test]
        [TestCase(typeof(bool))]
        [TestCase(typeof(Vector2))]
        [TestCase(typeof(SerializedType))]
        public void CheckPlainTypes(Type t) {
            AssertTypeCanBeSerialized(t);
        }

        [Test]
        [TestCase(typeof(bool[]))]
        [TestCase(typeof(Vector2[]))]
        [TestCase(typeof(SerializedType[]))]
        [TestCase(typeof(bool[][]))]
        [TestCase(typeof(Vector2[][][]))]
        public void CheckArrayTypes(Type t) {
            AssertTypeCanBeSerialized(t);
        }

        [Test]
        [TestCase(typeof(List<int>))]
        [TestCase(typeof(Func<SerializedType>))]
        [TestCase(typeof(Dictionary<Vector2, string>))]
        public void CheckSimpleGenericTypes(Type t) {
            AssertTypeCanBeSerialized(t);
        }

        [Test]
        [TestCase(typeof(List<int>[]))]
        [TestCase(typeof(Func<SerializedType>[]))]
        [TestCase(typeof(Dictionary<Vector2, string>[]))]
        public void CheckArrayOfSimpleGenericTypes(Type t) {
            AssertTypeCanBeSerialized(t);
        }

        [Test]
        [TestCase(typeof(List<int[]>))]
        [TestCase(typeof(Func<SerializedType[]>))]
        [TestCase(typeof(Dictionary<Vector2[], string[]>))]
        public void CheckSimpleGenericOfArrayTypes(Type t) {
            AssertTypeCanBeSerialized(t);
        }

        [Test]
        [TestCase(typeof(List<Func<SerializedType[]>[]>))]
        [TestCase(typeof(Func<Dictionary<int, bool[]>>[]))]
        public void CheckComplexGenericTypes(Type t) {
            AssertTypeCanBeSerialized(t);
        }

        [Test]
        [TestCase(typeof(Func<>))]
        [TestCase(typeof(Action<,>))]
        [TestCase(typeof(List<>))]
        public void CheckNonConcreteGenericTypes(Type t) {
            AssertTypeCanBeSerialized(t);
        }

        private static void AssertTypeCanBeSerialized(Type t) {
            var serializedType = new SerializedType(t);
            var deserializedType = serializedType.ToType();
            Assert.AreEqual(t, deserializedType);
        }
    }
}
