using InjeCtor.Core.Registration;
using InjeCtor.Core.Test.Interfaces;
using InjeCtor.Core.Test.TestClasses;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SutScope = InjeCtor.Core.Scope.Scope;

namespace InjeCtor.Core.Test.Scope
{
    [TestFixture]
    public class ScopeTest
    {
        #region Private Fields

        private DummyTypeMapper mTypeMapper;
        private DummyCreator mCreator;
        private DummyTypeInformationProvider mInformationProvider;
        private SutScope mScope;

        #endregion

        #region SetUp / TearDown

        [SetUp]
        public void SetUp()
        {
            mTypeMapper = new DummyTypeMapper();
            mCreator = new DummyCreator();
            mInformationProvider = new DummyTypeInformationProvider();

            mScope = new SutScope();
            mScope.MappingProvider = mTypeMapper;
            mScope.Creator = mCreator;
            mScope.TypeInformationProvider = mInformationProvider;
        }

        #endregion

        #region Tests

        [Test]
        public void Create_MappingProviderNull_ThrowsInvalidOperationException()
        {
            mScope.MappingProvider = null;

            Assert.Throws<InvalidOperationException>(() => mScope.Create<ICalculator>());
        }

        [Test]
        public void Create_MappingIsNull_ThrowsInvalidOperationException()
        {
            Assert.Throws<InvalidOperationException>(() => mScope.Create<UnknownClass>());
        }

        [Test]
        public void Create_MappedTypeIsNull_ThrowsInvalidOperationException()
        {
            Assert.Throws<InvalidOperationException>(() => mScope.Create<ClassWithoutMappedType>());
        }

        [Test]
        public void Create_CreatorNull_ThrowsInvalidOperationException()
        {
            mScope.Creator = null;

            Assert.Throws<InvalidOperationException>(() => mScope.Create<ICalculator>());
        }

        [TestCase(typeof(ICalculator), typeof(Calculator))]
        [TestCase(typeof(IGreeter), typeof(Greeter))]
        public void Create_MultipleTimesWithAlwaysCreationInstruction_SuccessWithNewInstances(Type requestType, Type resultType)
        {
            var createdObject = mScope.Create(requestType);
            var secondObject = mScope.Create(requestType);

            Assert.That(createdObject, Is.Not.Null);
            Assert.That(createdObject, Is.InstanceOf(resultType));
            Assert.That(secondObject, Is.Not.Null);
            Assert.That(secondObject, Is.InstanceOf(resultType));
            Assert.That(createdObject, Is.Not.SameAs(secondObject));
        }

        [TestCase(typeof(ICalculator), typeof(Calculator))]
        [TestCase(typeof(IGreeter), typeof(Greeter))]
        public void Create_MultipleTimesWithScopeCreationInstruction_SuccessWithNewInstances(Type requestType, Type resultType)
        {
            mTypeMapper.SetCreationInstruction(CreationInstruction.Scope);
            var createdObject = mScope.Create(requestType);
            var secondObject = mScope.Create(requestType);

            Assert.That(createdObject, Is.Not.Null);
            Assert.That(createdObject, Is.InstanceOf(resultType));
            Assert.That(secondObject, Is.Not.Null);
            Assert.That(secondObject, Is.InstanceOf(resultType));
            Assert.That(createdObject, Is.SameAs(secondObject));
        }

        [TestCase(typeof(ICalculator), typeof(Calculator))]
        [TestCase(typeof(IGreeter), typeof(Greeter))]
        public void Create_MultipleTimesWithSingletonCreationInstruction_SuccessWithNewInstances(Type requestType, Type resultType)
        {
            mTypeMapper.SetCreationInstruction(CreationInstruction.Singleton);

            Dictionary<Type, object> singletonInstance = new Dictionary<Type, object>();
            mScope.SetSingletons(singletonInstance);

            int creationCounter = 0;
            mScope.RequestSingletonCreationInstance += (sender, args) =>
            {
                creationCounter++;
                if (typeof(ICalculator).FullName == args.Type.FullName)
                    args.Instance = new Calculator();
                else if (typeof(IGreeter).FullName == args.Type.FullName)
                    args.Instance = new Greeter();

                singletonInstance[args.Type] = args.Instance;
            };

            var createdObject = mScope.Create(requestType);
            var secondObject = mScope.Create(requestType);

            Assert.That(createdObject, Is.Not.Null);
            Assert.That(createdObject, Is.InstanceOf(resultType));
            Assert.That(secondObject, Is.Not.Null);
            Assert.That(secondObject, Is.InstanceOf(resultType));
            Assert.That(createdObject, Is.SameAs(secondObject));
            Assert.That(creationCounter, Is.EqualTo(1));
        }

        [Test]
        public void GetSingleton_NoCreatedSingletonFound_NullValueReturend()
        {
            Assert.That(mScope.GetSingleton<BaseClassForSingleton>(), Is.Null);
        }

        [Test]
        public void GetSingleton_WithCreatedScopeSingleton_ReturnsInstance()
        {
            mTypeMapper.SetCreationInstruction(CreationInstruction.Scope);
            object instance = mScope.Create<BaseClassForSingleton>();

            Assert.That(mScope.GetSingleton<BaseClassForSingleton>(), Is.SameAs(instance));
        }

        [Test]
        public void GetSingleton_WithCreatedSingleton_ReturnsInstance()
        {
            Dictionary<Type, object> singletonInstance = new Dictionary<Type, object>();
            mScope.SetSingletons(singletonInstance);

            int creationCounter = 0;
            mScope.RequestSingletonCreationInstance += (sender, args) =>
            {
                creationCounter++;
                if (typeof(BaseClassForSingleton).FullName == args.Type.FullName)
                    args.Instance = new SingletonClass();

                singletonInstance[args.Type] = args.Instance;
            };

            object instance = mScope.Create<BaseClassForSingleton>();

            Assert.That(mScope.GetSingleton<BaseClassForSingleton>(), Is.SameAs(instance));
            Assert.That(creationCounter, Is.EqualTo(1));
        }

        [Test]
        public void Dispose_SingletonDisposedAndEventCalled_Success()
        {
            int dispingEventCounter = 0;
            mScope.Disposing += (sender, args) => dispingEventCounter++;

            mTypeMapper.SetCreationInstruction(CreationInstruction.Scope);
            var singletonInstance = mScope.Create<BaseClassForSingleton>(); // need to create a scope singleton to test if it gets disposed

            Assert.That(singletonInstance, Is.Not.Null);
            Assert.That(singletonInstance.IsDisposed, Is.False);
            Assert.That(dispingEventCounter, Is.EqualTo(0));

            mScope.Dispose();

            Assert.That(singletonInstance.IsDisposed, Is.True);
            Assert.That(dispingEventCounter, Is.EqualTo(1));
        }

        #endregion

        #region Private Test Classes

        class UnknownClass { }

        #endregion
    }
}
