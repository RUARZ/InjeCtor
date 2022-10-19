using InjeCtor.Core.Scope;
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

            SingletonClass.ResetCounter();
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

        [Test]
        public void Create_MultipleTimesWithCreationInstruction_SuccessWithNewInstances()
        {
            var firstObject = mInjeCtor.Create<BaseClassForSingleton>();
            var secondObject = mInjeCtor.Create<BaseClassForSingleton>();

            Assert.That(firstObject, Is.Not.Null);
            Assert.That(firstObject, Is.InstanceOf<SingletonClass>());
            Assert.That(secondObject, Is.Not.Null);
            Assert.That(secondObject, Is.InstanceOf<SingletonClass>());
            Assert.That(firstObject, Is.SameAs(secondObject));
            Assert.That(SingletonClass.CreationCounter, Is.EqualTo(1));

            Assert.That(secondObject.IsDisposed, Is.False);

            mInjeCtor.Dispose();

            Assert.That(secondObject.IsDisposed, Is.True);
        }

        [Test]
        public void Create_ScopeSingletonsFromDifferentScopes_SingletonWithinScopesButDifferentInOtherScope()
        {
            void AssertInstanceOfCalculator(object instance)
            {
                Assert.That(instance, Is.Not.Null);
                Assert.That(instance, Is.InstanceOf<Calculator>());
            }

            using (IScope scope = mInjeCtor.CreateScope())
            {
                var firstObjectDefaultScope = mInjeCtor.Create<ICalculator>();
                var secondObjectDefaultScope = mInjeCtor.Create<ICalculator>();

                var firstObjectScope = scope.Create<ICalculator>();
                var secondObjectScope = scope.Create<ICalculator>();

                using (IScope scope2 = mInjeCtor.CreateScope())
                {
                    var firstObjectScope2 = scope2.Create<ICalculator>();
                    var secondObjectScope2 = scope2.Create<ICalculator>();

                    // assert default scope objects
                    AssertInstanceOfCalculator(firstObjectDefaultScope);
                    AssertInstanceOfCalculator(secondObjectDefaultScope);
                    Assert.That(firstObjectDefaultScope, Is.SameAs(secondObjectDefaultScope));

                    // assert scope objects
                    AssertInstanceOfCalculator(firstObjectScope);
                    AssertInstanceOfCalculator(secondObjectScope);
                    Assert.That(firstObjectScope, Is.SameAs(secondObjectScope));

                    // assert scope2 objects
                    AssertInstanceOfCalculator(firstObjectScope2);
                    AssertInstanceOfCalculator(secondObjectScope2);
                    Assert.That(firstObjectScope2, Is.SameAs(secondObjectScope2));

                    // assert changes between scopes
                    Assert.That(firstObjectScope, Is.Not.SameAs(firstObjectDefaultScope));
                    Assert.That(firstObjectScope2, Is.Not.SameAs(firstObjectDefaultScope));
                    Assert.That(firstObjectScope2, Is.Not.SameAs(firstObjectScope));
                    Assert.That(secondObjectScope, Is.Not.SameAs(secondObjectDefaultScope));
                    Assert.That(secondObjectScope2, Is.Not.SameAs(secondObjectDefaultScope));
                    Assert.That(secondObjectScope2, Is.Not.SameAs(secondObjectScope));

                    // assert scopes still alive
                    Assert.That(mInjeCtor.GetScopes().Count(), Is.EqualTo(2));
                }

                var thirdObjectScope = scope.Create<ICalculator>();

                // assert scope objects
                AssertInstanceOfCalculator(thirdObjectScope);
                Assert.That(firstObjectScope, Is.SameAs(secondObjectScope));
                Assert.That(firstObjectScope, Is.SameAs(thirdObjectScope));
                Assert.That(secondObjectScope, Is.SameAs(thirdObjectScope));

                Assert.That(thirdObjectScope, Is.Not.SameAs(firstObjectDefaultScope));

                // assert scope still alive
                Assert.That(mInjeCtor.GetScopes().Count(), Is.EqualTo(1));
            }

            Assert.That(mInjeCtor.GetScopes().Count(), Is.EqualTo(0));
        }

        [Test]
        public void Create_SingletonsFromMultipleScopesAndOnlyDisposeInjeCtor_OnlyOneInstanceCreatedAndAllScopesDisposed()
        {
            var firstObject = mInjeCtor.Create<BaseClassForSingleton>();
            var secondObject = mInjeCtor.Create<BaseClassForSingleton>();

            IScope scope1 = mInjeCtor.CreateScope();
            IScope scope2 = mInjeCtor.CreateScope();

            var firstObjectScope1 = scope1.Create<BaseClassForSingleton>();
            var secondObjectScope1 = scope1.Create<BaseClassForSingleton>();

            var firstObjectScope2 = scope2.Create<BaseClassForSingleton>();
            var secondObjectScope2 = scope2.Create<BaseClassForSingleton>();

            Assert.That(firstObject, Is.Not.Null);
            Assert.That(firstObject, Is.InstanceOf<SingletonClass>());
            Assert.That(secondObject, Is.Not.Null);
            Assert.That(secondObject, Is.InstanceOf<SingletonClass>());

            Assert.That(firstObjectScope1, Is.Not.Null);
            Assert.That(firstObjectScope1, Is.InstanceOf<SingletonClass>());
            Assert.That(secondObjectScope1, Is.Not.Null);
            Assert.That(secondObjectScope1, Is.InstanceOf<SingletonClass>());

            Assert.That(firstObjectScope2, Is.Not.Null);
            Assert.That(firstObjectScope2, Is.InstanceOf<SingletonClass>());
            Assert.That(secondObjectScope2, Is.Not.Null);
            Assert.That(secondObjectScope2, Is.InstanceOf<SingletonClass>());

            Assert.That(firstObject, Is.SameAs(secondObject));
            Assert.That(firstObjectScope1, Is.SameAs(secondObjectScope1));
            Assert.That(firstObjectScope2, Is.SameAs(secondObjectScope2));
            Assert.That(firstObject, Is.SameAs(firstObjectScope1));
            Assert.That(
                new object[] 
                { firstObject, secondObject, firstObjectScope1, secondObjectScope1, firstObjectScope2, secondObjectScope2 }
                , Is.All.SameAs(firstObject));
            Assert.That(SingletonClass.CreationCounter, Is.EqualTo(1));

            Assert.That(secondObject.IsDisposed, Is.False);
            Assert.That(mInjeCtor.GetScopes().Count(), Is.EqualTo(2));

            mInjeCtor.Dispose();

            Assert.That(secondObject.IsDisposed, Is.True);
            Assert.That(mInjeCtor.GetScopes().Count(), Is.EqualTo(0));
        }

        #endregion
    }
}
