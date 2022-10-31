using InjeCtor.Core.Exceptions;
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
        private MethodInvocations mObject;

        #endregion

        #region SetUp / TearDown

        [SetUp]
        public void SetUp()
        {
            mObject = new MethodInvocations();

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
            var firstObject = mInjeCtor.Get<IGreeter>();
            var secondObject = mInjeCtor.Get<IGreeter>();

            Assert.That(firstObject, Is.Not.Null);
            Assert.That(firstObject, Is.InstanceOf<Greeter>());
            Assert.That(secondObject, Is.Not.Null);
            Assert.That(secondObject, Is.InstanceOf<Greeter>());
            Assert.That(firstObject, Is.Not.SameAs(secondObject));
        }

        [Test]
        public void Create_MultipleTimesWithScopeCreationInstruction_SuccessWithNewInstances()
        {
            var firstObject = mInjeCtor.Get<ICalculator>();
            var secondObject = mInjeCtor.Get<ICalculator>();

            Assert.That(firstObject, Is.Not.Null);
            Assert.That(firstObject, Is.InstanceOf<Calculator>());
            Assert.That(secondObject, Is.Not.Null);
            Assert.That(secondObject, Is.InstanceOf<Calculator>());
            Assert.That(firstObject, Is.SameAs(secondObject));
        }

        [Test]
        public void Create_MultipleTimesWithCreationInstruction_SuccessWithNewInstances()
        {
            var firstObject = mInjeCtor.Get<BaseClassForSingleton>();
            var secondObject = mInjeCtor.Get<BaseClassForSingleton>();

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

            var firstObject = mInjeCtor.Get<IEnumerable<int>>();
            var secondObject = mInjeCtor.Get<IEnumerable<int>>();

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
                var firstObjectDefaultScope = mInjeCtor.Get<ICalculator>();
                var secondObjectDefaultScope = mInjeCtor.Get<ICalculator>();

                var firstObjectScope = scope.Get<ICalculator>();
                var secondObjectScope = scope.Get<ICalculator>();

                using (IScope scope2 = mInjeCtor.CreateScope())
                {
                    var firstObjectScope2 = scope2.Get<ICalculator>();
                    var secondObjectScope2 = scope2.Get<ICalculator>();

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

                var thirdObjectScope = scope.Get<ICalculator>();

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
            var firstObject = mInjeCtor.Get<BaseClassForSingleton>();
            var secondObject = mInjeCtor.Get<BaseClassForSingleton>();

            IScope scope1 = mInjeCtor.CreateScope();
            IScope scope2 = mInjeCtor.CreateScope();

            var firstObjectScope1 = scope1.Get<BaseClassForSingleton>();
            var secondObjectScope1 = scope1.Get<BaseClassForSingleton>();

            var firstObjectScope2 = scope2.Get<BaseClassForSingleton>();
            var secondObjectScope2 = scope2.Get<BaseClassForSingleton>();

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

            var createdObject = mInjeCtor.Get<IDummyInterface>();

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
            var createdObject = mInjeCtor.Get<NotMappedClass>();

            Assert.That(createdObject, Is.Not.Null);
            Assert.That(createdObject.Calculator, Is.Not.Null);
            Assert.That(createdObject.Greeter, Is.Not.Null);
            Assert.That(createdObject.Calculator, Is.InstanceOf<Calculator>());
            Assert.That(createdObject.Greeter, Is.InstanceOf<Greeter>());
        }

        [Test]
        public void Create_NotMappedInterfaceAndAbstractClass_InvalidOperationException()
        {
            Assert.Throws<InvalidOperationException>(() => mInjeCtor.Get<INotMappedInterface>());
            Assert.Throws<InvalidOperationException>(() => mInjeCtor.Get<NotMappedAbstractClass>());
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

            var createdInstance = mInjeCtor.Get<NotMappedClassWithInjections>();

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
            var createdObject = mInjeCtor.Get<NotMappedClassWithAnotherNotMappedClassForCtor>();

            using IScope scope = mInjeCtor.CreateScope();

            var scopeObject = scope.Get<NotMappedClassWithAnotherNotMappedClassForCtor>();

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

            var createdInstance = mInjeCtor.Get<NotMappedClassWithInjections>();
            var secondInstance = mInjeCtor.Get<NotMappedClassWithInjections>();

            using IScope scope = mInjeCtor.CreateScope();

            var otherScopeInstance = scope.Get<NotMappedClassWithInjections>();

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

            var createdInstance = mInjeCtor.Get<NotMappedClassWithInjections>();
            var secondInstance = mInjeCtor.Get<NotMappedClassWithInjections>();

            using IScope scope = mInjeCtor.CreateScope();

            var otherScopeInstance = scope.Get<NotMappedClassWithInjections>();

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

        [Test]
        public void Create_SimpleCircularReference_CircularReferenceExceptionThrown()
        {
            mInjeCtor.Mapper.Add<ICircularReferenceInterfaceA>().As<SimpleCircularReferenceClassA>();
            mInjeCtor.Mapper.Add<ICircularReferenceInterfaceB>().As<SimpleCircularReferenceClassB>();

            Assert.Throws<CircularReferenceException>(() => mInjeCtor.Get<ICircularReferenceInterfaceA>());
        }

        [Test]
        public void Create_CircularReference_CircularReferenceExceptionThrown()
        {
            mInjeCtor.Mapper.Add<ICircularReferenceInterfaceA>().As<CircularReferenceClassA>();
            mInjeCtor.Mapper.Add<ICircularReferenceInterfaceB>().As<CircularReferenceClassB>();
            mInjeCtor.Mapper.Add<ICircularReferenceInterfaceC>().As<CircularReferenceClassC>();
            mInjeCtor.Mapper.Add<ICircularReferenceInterfaceD>().As<CircularReferenceClassD>();

            Assert.Throws<CircularReferenceException>(() => mInjeCtor.Get<ICircularReferenceInterfaceA>());
        }

        [Test]
        public void Create_CircularReferenceClassesWithoutCompleteCircularReference_InstanceCreated()
        {
            mInjeCtor.Mapper.Add<ICircularReferenceInterfaceA>().As<CircularReferenceClassA>();
            mInjeCtor.Mapper.Add<ICircularReferenceInterfaceB>().As<CircularReferenceClassB>();
            mInjeCtor.Mapper.Add<ICircularReferenceInterfaceC>().As<CircularReferenceClassC>();

            var createdObject = mInjeCtor.Get<ICircularReferenceInterfaceA>();

            Assert.That(createdObject, Is.Not.Null);
            Assert.That(createdObject, Is.InstanceOf<CircularReferenceClassA>());
        }

        [Test]
        public void Invoke_NoAdditionalParameters_Success()
        {
            object? result = mInjeCtor.Invoke(mObject, o => o.Greet);

            Assert.That(mObject.Greeter, Is.Not.Null);
            Assert.That(mObject.LastGreeting, Is.EqualTo("Greetings to 'Herbert'!"));
            Assert.That(result, Is.Null);
        }

        [Test]
        public void Invoke_AdditionalParameters_Success()
        {
            object? result = mInjeCtor.Invoke(mObject, o => o.Subtract, 48, 6);

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
            object? result = mInjeCtor.Invoke(mObject, o => o.MultipleDifferentParameters, number1, number2, number3, name);

            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<int>());
            Assert.That(result, Is.EqualTo(result));
            Assert.That(mObject.LastGreeting, Is.EqualTo($"Greetings to '{name}'!"));
        }

        [Test]
        public void Invoke_StaticMethod_Success()
        {
            object? result = mInjeCtor.Invoke(() => StaticMethodInvocations.Add, 38, 4);

            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<int>());
            Assert.That(result, Is.EqualTo(42));
        }

        [TestCase(2, 3, 10, "Some Name", 50)]
        [TestCase(22, 18, 2, "Another Name", 80)]
        [TestCase(33, 2, 8, "Some Name", 280)]
        public void Invoke_StaticMethodMultipleAdditionalParameters_Success(int number1, int number2, int number3, string name, int expectedResult)
        {
            object? result = mInjeCtor.Invoke(() => StaticMethodInvocations.MultipleDifferentParameters, number1, number2, number3, name);

            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<int>());
            Assert.That(result, Is.EqualTo(result));
            Assert.That(StaticMethodInvocations.LastGreeting, Is.EqualTo($"Greetings to '{name}'!"));
        }

        [Test]
        public void CreateScope_DifferentScopesWithCorrectInstances()
        {
            using IScope scope1 = mInjeCtor.CreateScope();
            using IScope scope2 = mInjeCtor.CreateScope();

            Assert.That(scope1, Is.Not.Null);
            Assert.That(scope2, Is.Not.Null);
            Assert.That(scope1, Is.Not.SameAs(scope2));

            Assert.That(scope1.Creator, Is.Not.Null);
            Assert.That(scope2.Creator, Is.Not.Null);
            Assert.That(scope1.Creator, Is.SameAs(scope2.Creator));

            Assert.That(scope1.MappingProvider, Is.Not.Null);
            Assert.That(scope2.MappingProvider, Is.Not.Null);
            Assert.That(scope1.MappingProvider, Is.SameAs(scope2.MappingProvider));

            Assert.That(scope1.TypeInformationProvider, Is.Not.Null);
            Assert.That(scope2.TypeInformationProvider, Is.Not.Null);
            Assert.That(scope1.TypeInformationProvider, Is.SameAs(scope2.TypeInformationProvider));

            Assert.That(scope1.Invoker, Is.Not.Null);
            Assert.That(scope2.Invoker, Is.Not.Null);
            Assert.That(scope1.Invoker, Is.Not.SameAs(scope2.Invoker));
        }

        [Test]
        public void Create_InjeCtorAndScopeInjectionWithCtor_InstancesInjected()
        {
            using IScope scope = mInjeCtor.CreateScope();

            var instance1 = mInjeCtor.Get<ScopeAndInjeCtorInjectionsCtor>();
            var instance2 = scope.Get<ScopeAndInjeCtorInjectionsCtor>();

            Assert.That(instance1, Is.Not.SameAs(instance2));

            Assert.That(instance1.Injector, Is.Not.Null);
            Assert.That(instance1.Scope, Is.Not.Null);

            Assert.That(instance2.Injector, Is.Not.Null);
            Assert.That(instance2.Scope, Is.Not.Null);

            Assert.That(instance1.Scope, Is.Not.SameAs(instance2.Scope));
            Assert.That(instance2.Scope, Is.SameAs(scope));
            Assert.That(instance1.Injector, Is.SameAs(instance2.Injector));
            Assert.That(instance1.Injector, Is.SameAs(mInjeCtor));
        }

        [Test]
        public void Create_InjeCtorAndScopeInjectionWithTypeInformation_InstancesInjected()
        {
            mInjeCtor.Mapper.Add<IDummyInterface>().As<ScopeAndInjeCtorInjectionsProperties>();

            using IScope scope = mInjeCtor.CreateScope();

            var createdInstance1 = mInjeCtor.Get<IDummyInterface>();
            var createdInstance2 = scope.Get<IDummyInterface>();

            var instance1 = AssertAndGetCastedType<ScopeAndInjeCtorInjectionsProperties>(createdInstance1);
            var instance2 = AssertAndGetCastedType<ScopeAndInjeCtorInjectionsProperties>(createdInstance2);

            Assert.That(instance1, Is.Not.SameAs(instance2));

            Assert.That(instance1.Injector, Is.Not.Null);
            Assert.That(instance1.Scope, Is.Not.Null);

            Assert.That(instance2.Injector, Is.Not.Null);
            Assert.That(instance2.Scope, Is.Not.Null);

            Assert.That(instance1.Scope, Is.Not.SameAs(instance2.Scope));
            Assert.That(instance2.Scope, Is.SameAs(scope));
            Assert.That(instance1.Injector, Is.SameAs(instance2.Injector));
            Assert.That(instance1.Injector, Is.SameAs(mInjeCtor));
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
