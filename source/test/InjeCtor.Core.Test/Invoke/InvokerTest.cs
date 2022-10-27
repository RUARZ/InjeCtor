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
        private MethodInvokations mObject;

        #endregion

        #region SetUp / TearDown

        [SetUp]
        public void SetUp()
        {
            mObject = new MethodInvokations();

            mScope = new DummyScope();
            mCreator = new DummyCreator();
            mTypeMapper = new DummyTypeMapper();

            mScope.MappingProvider = mTypeMapper;
            mCreator.MappingProvider = mTypeMapper;
            mScope.Creator = mCreator;

            mInvoker = new Invoker();

            mInvoker.Scope = mScope;
        }

        #endregion

        #region Tests

        [Test]
        public void Invoke_NoAdditionalParameters_Success()
        {
            mInvoker.Invoke(mObject, o => o.Greet);

            Assert.That(mObject.Greeter, Is.Not.Null);
            Assert.That(mObject.LastGreeting, Is.EqualTo("Greetings to 'Herbert'!"));
        }

        [Test]
        public void Invoke_AdditionalParameters_Success()
        {
            mInvoker.Invoke(mObject, o => o.Subtract, 48, 6);

            Assert.That(mObject.LastCalculationResult, Is.EqualTo(42));
        }

        #endregion
    }
}
