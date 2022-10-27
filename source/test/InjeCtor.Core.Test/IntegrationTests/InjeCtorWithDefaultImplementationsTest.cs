using InjeCtor.Core.Registration;
using InjeCtor.Core.Scope;
using InjeCtor.Core.Test.Interfaces;
using InjeCtor.Core.Test.TestClasses;
using InjeCtor.Core.TypeInformation;
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
            mInjeCtor.Mapper.Add<ICalculator>().AsScopeSingleton<Calculator>();
            mInjeCtor.Mapper.Add<BaseClassForSingleton>().AsSingleton<SingletonClass>();

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
        public void Create_SingletonWithSetSingletonInstance_PassedSingletonUsed()
        {
            int[] myIntArray = new int[] { 1, 2, 3, 4, 5 };
            mInjeCtor.Mapper.Add<IEnumerable<int>>().AsSingleton(myIntArray);

            var firstObject = mInjeCtor.Create<IEnumerable<int>>();
            var secondObject = mInjeCtor.Create<IEnumerable<int>>();

            Assert.That(firstObject, Is.Not.Null);
            Assert.That(firstObject, Is.InstanceOf<IEnumerable<int>>());
            Assert.That(secondObject, Is.Not.Null);
            Assert.That(secondObject, Is.InstanceOf<IEnumerable<int>>());
            Assert.That(firstObject, Is.SameAs(secondObject));
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

        [TestCase(false, CreationInstruction.Always)]
        [TestCase(true, CreationInstruction.Always)]
        [TestCase(false, CreationInstruction.Scope)]
        [TestCase(true, CreationInstruction.Scope)]
        [TestCase(false, CreationInstruction.Singleton)]
        [TestCase(true, CreationInstruction.Singleton)]
        public void Create_WithTypeInformations_PropertyInjected(bool addTypeInformationForNonAttributeProperty, CreationInstruction creationInstruction)
        {
            ITypeMapping<IDummyInterface> mapping = mInjeCtor.Mapper.Add<IDummyInterface>();
            switch (creationInstruction)
            {
                case CreationInstruction.Always:
                    mapping.As<DummyClassWithInjectAttributes>();
                    break;
                case CreationInstruction.Scope:
                    mapping.AsScopeSingleton<DummyClassWithInjectAttributes>();
                    break;
                case CreationInstruction.Singleton:
                    mapping.AsSingleton<DummyClassWithInjectAttributes>();
                    break;
            }

            if (addTypeInformationForNonAttributeProperty)
                mInjeCtor.TypeInformationBuilder.AddPropertyInjection((DummyClassWithInjectAttributes c) => c.Greeter);

            var createdObject = mInjeCtor.Create<IDummyInterface>();

            var instance = AssertAndGetCastedType<DummyClassWithInjectAttributes>(createdObject);

            if (!addTypeInformationForNonAttributeProperty)
            {
                Assert.That(instance.Greeter, Is.Null);
            }
            else
            {
                Assert.That(instance.Greeter, Is.Not.Null);
                Assert.That(instance.Greeter, Is.InstanceOf<Greeter>());
            }
            Assert.That(instance.GreeterWithAttribute, Is.Not.Null);
            Assert.That(instance.GreeterWithAttribute, Is.InstanceOf<Greeter>());
        }

        [Test]
        public void Create_NotAddedTypeMappingClass_InstanceCreatedWithPassedParameters()
        {
            var createdObject = mInjeCtor.Create<NotMappedClass>();

            Assert.That(createdObject, Is.Not.Null);
            Assert.That(createdObject.Calculator, Is.Not.Null);
            Assert.That(createdObject.Greeter, Is.Not.Null);
            Assert.That(createdObject.Calculator, Is.InstanceOf<Calculator>());
            Assert.That(createdObject.Greeter, Is.InstanceOf<Greeter>());
        }

        [Test]
        public void Create_NotMappedInterfaceAndAbstractClass_InvalidOperationException()
        {
            Assert.Throws<InvalidOperationException>(() => mInjeCtor.Create<INotMappedInterface>());
            Assert.Throws<InvalidOperationException>(() => mInjeCtor.Create<NotMappedAbstractClass>());
        }

        [TestCase(false, false)]
        [TestCase(true, false)]
        [TestCase(false, true)]
        [TestCase(true, true)]
        public void Create_NotMappedClassWithInjections_InjectedDependingOnTypeInformation(bool setTypeInformation, bool setPropertyInjectionWithExpression)
        {
            if (setTypeInformation)
                mInjeCtor.TypeInformationBuilder.Add<NotMappedClassWithInjections>();

            if (setPropertyInjectionWithExpression)
                mInjeCtor.TypeInformationBuilder.AddPropertyInjection((NotMappedClassWithInjections c) => c.Greeter);

            var createdInstance = mInjeCtor.Create<NotMappedClassWithInjections>();

            Assert.That(createdInstance, Is.Not.Null);
            Assert.That(createdInstance, Is.TypeOf<NotMappedClassWithInjections>());
            
            if (setTypeInformation)
            {
                Assert.That(createdInstance.Calculator, Is.Not.Null);
                Assert.That(createdInstance.Calculator, Is.InstanceOf<Calculator>());
            }
            else
            {
                Assert.That(createdInstance.Calculator, Is.Null);
            }

            if (setPropertyInjectionWithExpression)
            {
                Assert.That(createdInstance.Greeter, Is.Not.Null);
                Assert.That(createdInstance.Greeter, Is.InstanceOf<Greeter>());
            }
            else
            {
                Assert.That(createdInstance.Greeter, Is.Null);
            }
        }

        [Test]
        public void Create_NotMappedClassWithOtherNotMappedClassAsCtorParameter_InstanceCreated()
        {
            var createdObject = mInjeCtor.Create<NotMappedClassWithAnotherNotMappedClassForCtor>();

            using IScope scope = mInjeCtor.CreateScope();

            var scopeObject = scope.Create<NotMappedClassWithAnotherNotMappedClassForCtor>();

            void AssertInstance(NotMappedClassWithAnotherNotMappedClassForCtor instance)
            {
                Assert.That(instance, Is.Not.Null);
                Assert.That(instance.OtherClass, Is.Not.Null);
                Assert.That(instance.OtherClass.Greeter, Is.Not.Null);
                Assert.That(instance.OtherClass.Calculator, Is.Not.Null);
                Assert.That(instance.OtherClass.Greeter, Is.InstanceOf<Greeter>());
                Assert.That(instance.OtherClass.Calculator, Is.InstanceOf<Calculator>());
            }

            AssertInstance(createdObject);
            AssertInstance(scopeObject);
        }

        [TestCase(CreationInstruction.Always)]
        [TestCase(CreationInstruction.Scope)]
        [TestCase(CreationInstruction.Singleton)]
        public void Create_PreviouslyNotMappedTypeAddedDirectly_InstanceCreated(CreationInstruction instruction)
        {
            switch (instruction)
            {
                case CreationInstruction.Always:
                    mInjeCtor.Mapper.AddTransient<NotMappedClassWithInjections>();
                    break;
                case CreationInstruction.Scope:
                    mInjeCtor.Mapper.AddScopeSingleton<NotMappedClassWithInjections>();
                    break;
                case CreationInstruction.Singleton:
                    mInjeCtor.Mapper.AddSingleton<NotMappedClassWithInjections>();
                    break;
            }

            var createdInstance = mInjeCtor.Create<NotMappedClassWithInjections>();
            var secondInstance = mInjeCtor.Create<NotMappedClassWithInjections>();

            using IScope scope = mInjeCtor.CreateScope();

            var otherScopeInstance = scope.Create<NotMappedClassWithInjections>();

            void AssertInstance(NotMappedClassWithInjections instance)
            {
                Assert.That(instance, Is.Not.Null);
                Assert.That(instance, Is.TypeOf<NotMappedClassWithInjections>());

                Assert.That(instance.Calculator, Is.Not.Null);
                Assert.That(instance.Calculator, Is.InstanceOf<Calculator>());

                Assert.That(instance.Greeter, Is.Null);
            }

            AssertInstance(createdInstance);
            AssertInstance(secondInstance);
            AssertInstance(otherScopeInstance);

            switch (instruction)
            {
                case CreationInstruction.Always:
                    Assert.That(createdInstance, Is.Not.SameAs(secondInstance));
                    Assert.That(secondInstance, Is.Not.SameAs(otherScopeInstance));
                    break;
                case CreationInstruction.Scope:
                    Assert.That(createdInstance, Is.SameAs(secondInstance));
                    Assert.That(secondInstance, Is.Not.SameAs(otherScopeInstance));
                    break;
                case CreationInstruction.Singleton:
                    Assert.That(createdInstance, Is.SameAs(secondInstance));
                    Assert.That(secondInstance, Is.SameAs(otherScopeInstance));
                    break;
            }
        }

        [Test]
        public void Create_PreviouslyNotMappedTypeAddedDirectlyWithInstance_InstanceCreated()
        {
            NotMappedClassWithInjections instanceToUse = new NotMappedClassWithInjections();
            mInjeCtor.Mapper.AddSingleton(instanceToUse);

            var createdInstance = mInjeCtor.Create<NotMappedClassWithInjections>();
            var secondInstance = mInjeCtor.Create<NotMappedClassWithInjections>();

            using IScope scope = mInjeCtor.CreateScope();

            var otherScopeInstance = scope.Create<NotMappedClassWithInjections>();

            void AssertInstance(NotMappedClassWithInjections instance)
            {
                Assert.That(instance, Is.Not.Null);
                Assert.That(instance, Is.TypeOf<NotMappedClassWithInjections>());

                Assert.That(instance.Calculator, Is.Not.Null);
                Assert.That(instance.Calculator, Is.InstanceOf<Calculator>());

                Assert.That(instance.Greeter, Is.Null);
            }

            AssertInstance(createdInstance);
            AssertInstance(secondInstance);
            AssertInstance(otherScopeInstance);
            
            Assert.That(createdInstance, Is.SameAs(secondInstance));
            Assert.That(secondInstance, Is.SameAs(otherScopeInstance));
            Assert.That(createdInstance, Is.SameAs(instanceToUse));
        }

        #endregion

        #region Private Methods

        private T AssertAndGetCastedType<T>(object? instance)
        {
            Assert.That(instance, Is.Not.Null);
            Assert.That(instance, Is.InstanceOf(typeof(T)));

            return (T)instance;
        }

        #endregion
    }
}
