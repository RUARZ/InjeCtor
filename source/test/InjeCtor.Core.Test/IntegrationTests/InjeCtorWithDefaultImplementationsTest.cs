using InjeCtor.Core.Test.Interfaces;
using InjeCtor.Core.Test.TestClasses;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InjeCtor.Core.Test.IntegrationTests
{
    [TestFixture]
    public class InjeCtorWithDefaultImplementationsTest
    {
        #region Private Fields

        private IInjeCtor mInjeCtor;

        #endregion

        #region SetUp / TearDown

        [SetUp]
        public void SetUp()
        {
            mInjeCtor = new InjeCtor();

            mInjeCtor.Mapper.Add<IGreeter>().As<Greeter>();
            mInjeCtor.Mapper.Add<ICalculator>().As<Calculator>().AsScopeSingleton();
            mInjeCtor.Mapper.Add<BaseClassForSingleton>().As<SingletonClass>().AsSingleton();
        }

        #endregion

        #region Tests

        [Test]
        public void Create_MultipleTimesWithAlwaysCreationInstruction_SuccessWithNewInstances()
        {
            var firstObject = mInjeCtor.Create<IGreeter>();
            var secondObject = mInjeCtor.Create<IGreeter>();

            Assert.That(firstObject, Is.Not.Null);
            Assert.That(firstObject, Is.InstanceOf<Greeter>());
            Assert.That(secondObject, Is.Not.Null);
            Assert.That(secondObject, Is.InstanceOf<Greeter>());
            Assert.That(firstObject, Is.Not.SameAs(secondObject));
        }

        [Test]
        public void Create_MultipleTimesWithScopeCreationInstruction_SuccessWithNewInstances()
        {
            var firstObject = mInjeCtor.Create<ICalculator>();
            var secondObject = mInjeCtor.Create<ICalculator>();

            Assert.That(firstObject, Is.Not.Null);
            Assert.That(firstObject, Is.InstanceOf<Calculator>());
            Assert.That(secondObject, Is.Not.Null);
            Assert.That(secondObject, Is.InstanceOf<Calculator>());
            Assert.That(firstObject, Is.SameAs(secondObject));
        }

        #endregion
    }
}
