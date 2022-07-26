using MisterGames.Common.Data;
using NUnit.Framework;

namespace Model {

    public class ObservableTests {

        [Test]
        public void Notifies_WhenSettingNotEqual_Struct() {
            var struct0 = new TestStruct(0);
            var struct1 = new TestStruct(1);
            
            var observable = Observable<TestStruct>.From(struct0);
            observable.OnValueChanged += newValue => Assert.Pass();
            
            observable.Value = struct1;
            Assert.Fail();
        }
        
        [Test]
        public void Notifies_WhenSettingNotEqual_Class() {
            var class0 = new TestClass(0);
            var class1 = new TestClass(1);
            
            var observable = Observable<TestClass>.From(class0);
            observable.OnValueChanged += newValue => Assert.Pass();
            
            observable.Value = class1;
            Assert.Fail();
        }
        
        [Test]
        public void Notifies_WhenSettingNotEqual_Enum() {
            const TestEnum enum0 = TestEnum.TestEnumValue0;
            const TestEnum enum1 = TestEnum.TestEnumValue1;
            
            var observable = Observable<TestEnum>.From(enum0);
            observable.OnValueChanged += newValue => Assert.Pass();
            
            observable.Value = enum1;
            Assert.Fail();
        }
        
        [Test]
        public void NoNotification_WhenSettingEqual_Struct() {
            var struct0 = new TestStruct(0);
            var struct1 = new TestStruct(0);
            
            var observable = Observable<TestStruct>.From(struct0);
            observable.OnValueChanged += newValue => Assert.Fail();
            
            observable.Value = struct0;
            observable.Value = struct1;
        }
        
        [Test]
        public void NoNotification_WhenSettingEqual_Class() {
            var class0 = new TestClass(0);
            var class1 = new TestClass(0);
            
            var observable = Observable<TestClass>.From(class0);
            observable.OnValueChanged += newValue => Assert.Fail();
            
            observable.Value = class0;
            observable.Value = class1;
        }
        
        [Test]
        public void NoNotification_WhenSettingEqual_Enum() {
            const TestEnum enum0 = TestEnum.TestEnumValue0;
            const TestEnum enum1 = TestEnum.TestEnumValue0;
            
            var observable = Observable<TestEnum>.From(enum0);
            observable.OnValueChanged += newValue => Assert.Fail();
            
            observable.Value = enum0;
            observable.Value = enum1;
        }

        private struct TestStruct {
            public float value;
            public TestStruct(float value) {
                this.value = value;
            }
        }
        
        private struct TestClass {
            public float value;
            public TestClass(float value) {
                this.value = value;
            }
        }
        
        private enum TestEnum {
            TestEnumValue0,
            TestEnumValue1,
        }

    }

}