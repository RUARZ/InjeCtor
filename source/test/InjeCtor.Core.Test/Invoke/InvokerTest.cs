using InjeCtor.Core.Invoke;
using InjeCtor.Core.Test.TestClasses;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InjeCtor.Core.Test.Invoke
{
    [TestFixture]
    public class InvokerTest
    {
        #region Private Fields

        private Invoker mInvoker;
        private DummyScope mScope;
        private DummyCreator mCreator;
        private DummyTypeMapper mTypeMapper;
        private MethodInvocations mObject;

        #endregion

        #region SetUp / TearDown

        [SetUp]
        public void SetUp()
        {
            mObject = new MethodInvocations();

            mScope = new DummyScope();
            mCreator = new DummyCreator();
            mTypeMapper = new DummyTypeMapper();

            mScope.MappingProvider = mTypeMapper;
            mCreator.MappingProvider = mTypeMapper;
            mScope.Creator = mCreator;

            mInvoker = new Invoker();

            mInvoker.Scope = mScope;

            StaticMethodInvocations.LastGreeting = null;
        }

        #endregion

        #region Tests

        [Test]
        public void Invoke_NoAdditionalParameters_Success()
        {
            object? result = mInvoker.Invoke(mObject, o => o.Greet);

            Assert.That(mObject.Greeter, Is.Not.Null);
            Assert.That(mObject.LastGreeting, Is.EqualTo("Greetings to 'Herbert'!"));
            Assert.That(result, Is.Null);
        }

        [Test]
        public void Invoke_AdditionalParameters_Success()
        {
            object? result = mInvoker.Invoke(mObject, o => o.Subtract, 48, 6);

            Assert.That(mObject.LastCalculationResult, Is.EqualTo(42));
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<int>());
            Assert.That(result, Is.EqualTo(42));
        }

        [TestCase(2, 3, 10, "Some Name", 50)]
        [TestCase(22, 18, 2, "Another Name", 80)]
        [TestCase(33, 2, 8, "Some Name", 280)]
        public void Invoke_MultipleAdditionalParameters_Success(int number1, int number2, int number3, string name, int expectedResult)
        {
            object? result = mInvoker.Invoke(mObject, o => o.MultipleDifferentParameters, number1, number2, number3, name);

            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<int>());
            Assert.That(result, Is.EqualTo(result));
            Assert.That(mObject.LastGreeting, Is.EqualTo($"Greetings to '{name}'!"));
        }

        [Test]
        public void Invoke_StaticMethod_Success()
        {
            object? result = mInvoker.Invoke(() => StaticMethodInvocations.Add, 38, 4);

            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<int>());
            Assert.That(result, Is.EqualTo(42));
        }

        [TestCase(2, 3, 10, "Some Name", 50)]
        [TestCase(22, 18, 2, "Another Name", 80)]
        [TestCase(33, 2, 8, "Some Name", 280)]
        public void Invoke_StaticMethodMultipleAdditionalParameters_Success(int number1, int number2, int number3, string name, int expectedResult)
        {
            object? result = mInvoker.Invoke(() => StaticMethodInvocations.MultipleDifferentParameters, number1, number2, number3, name);

            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<int>());
            Assert.That(result, Is.EqualTo(result));
            Assert.That(StaticMethodInvocations.LastGreeting, Is.EqualTo($"Greetings to '{name}'!"));
        }

        #endregion
    }
}
