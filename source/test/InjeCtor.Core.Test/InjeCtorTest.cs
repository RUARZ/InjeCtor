using InjeCtor.Core.Creation;
using InjeCtor.Core.Registration;
using InjeCtor.Core.Scope;
using InjeCtor.Core.Test.Interfaces;
using InjeCtor.Core.Test.TestClasses;
using InjeCtor.Core.TypeInformation;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace InjeCtor.Core.Test
{
    [TestFixture]
    public class InjeCtorTest
    {
        #region Private Fields

        private DummyTypeMapper mTypeMapper;
        private DummyTypeInformationProvider mTypeInformationProvider;
        private DummyCreator mCreator;
        private InjeCtor mInjeCtor;

        #endregion

        #region SetUp / TearDown

        [SetUp]
        public void SetUp()
        {
            mTypeMapper = new DummyTypeMapper();
            mTypeInformationProvider = new DummyTypeInformationProvider();
            mCreator = new DummyCreator();
            DummyScope.ResetCounter();
            SingletonClass.ResetCounter();
            mInjeCtor = new InjeCtor(mTypeMapper, mTypeInformationProvider, mCreator);
        }

        #endregion

        #region Tests

        [TestCase(typeof(ICalculator), typeof(Calculator))]
        [TestCase(typeof(IGreeter), typeof(Greeter))]
        public void Create_MultipleTimesWithAlwaysCreationInstruction_SuccessWithNewInstances(Type requestType, Type resultType)
        {
            var createdObject = mInjeCtor.Create(requestType);
            var secondObject = mInjeCtor.Create(requestType);

            Assert.That(createdObject, Is.Not.Null);
            Assert.That(createdObject, Is.InstanceOf(resultType));
            Assert.That(secondObject, Is.Not.Null);
            Assert.That(secondObject, Is.InstanceOf(resultType));
            Assert.That(createdObject, Is.Not.SameAs(secondObject));
        }

        [TestCase(typeof(ICalculator), typeof(Calculator))]
        [TestCase(typeof(IGreeter), typeof(Greeter))]
        [TestCase(typeof(BaseClassForSingleton), typeof(SingletonClass))]
        public void Create_MultipleTimesWithSingletonCreationInstruction_SuccessWithSameInstances(Type requestType, Type resultType)
        {
            mTypeMapper.SetCreationInstruction(CreationInstruction.Singleton);
            var createdObject = mInjeCtor.Create(requestType);
            var secondObject = mInjeCtor.Create(requestType);

            Assert.That(createdObject, Is.Not.Null);
            Assert.That(createdObject, Is.InstanceOf(resultType));
            Assert.That(secondObject, Is.Not.Null);
            Assert.That(secondObject, Is.InstanceOf(resultType));
            Assert.That(createdObject, Is.SameAs(secondObject));
            Assert.That(mCreator.CreatedSingletons.Count, Is.EqualTo(1));
            Assert.That(mCreator.CreatedSingletons[requestType], Is.InstanceOf(resultType));
        }

        [Test]
        public void Create_MultipleTimesSingleton_OnlyOneTimeCreated()
        {
            var createdObject = mInjeCtor.Create<BaseClassForSingleton>();

            Assert.That(createdObject, Is.Not.Null);
            Assert.That(createdObject, Is.InstanceOf<BaseClassForSingleton>());
            Assert.That(createdObject, Is.InstanceOf<SingletonClass>());
            Assert.That(createdObject, Is.SameAs(mInjeCtor.Create<BaseClassForSingleton>()));
            Assert.That(createdObject, Is.SameAs(mInjeCtor.Create<BaseClassForSingleton>()));
            Assert.That(createdObject, Is.SameAs(mInjeCtor.Create<BaseClassForSingleton>()));
            Assert.That(createdObject, Is.SameAs(mInjeCtor.Create<BaseClassForSingleton>()));
            Assert.That(SingletonClass.CreationCounter, Is.EqualTo(1));
            Assert.That(mCreator.CreatedSingletons.Count, Is.EqualTo(1));
            Assert.That(mCreator.CreatedSingletons[typeof(BaseClassForSingleton)], Is.InstanceOf(typeof(SingletonClass)));
        }

        [Test]
        public void Create_SingletonWithPassedInstance_PassedInstanceUsed()
        {
            SingletonClass singleton = new SingletonClass();
            mTypeMapper.SetDirectSingleton(typeof(BaseClassForSingleton), singleton);

            var createdObject = mInjeCtor.Create<BaseClassForSingleton>();

            Assert.That(createdObject, Is.Not.Null);
            Assert.That(createdObject, Is.InstanceOf<BaseClassForSingleton>());
            Assert.That(createdObject, Is.InstanceOf<SingletonClass>());
            Assert.That(createdObject, Is.SameAs(mInjeCtor.Create<BaseClassForSingleton>()));
            Assert.That(singleton, Is.SameAs(mInjeCtor.Create<BaseClassForSingleton>()));
            Assert.That(createdObject, Is.SameAs(singleton));
            Assert.That(SingletonClass.CreationCounter, Is.EqualTo(1));
            Assert.That(mCreator.CreatedSingletons.Count, Is.EqualTo(1));
            Assert.That(mCreator.CreatedSingletons[typeof(BaseClassForSingleton)], Is.InstanceOf(typeof(SingletonClass)));
        }

        [Test]
        public void Create_MissingMappedType_ThrowsInvalidOperationException()
        {
            Assert.Throws<InvalidOperationException>(() => mInjeCtor.Create<ClassWithoutMappedType>());
        }

        [Test]
        public void PassedImplementations_GetPassedImplementations_SameAsPassed()
        {
            Assert.That(mInjeCtor.Mapper, Is.SameAs(mTypeMapper));
            Assert.That(mInjeCtor.MappingProvider, Is.SameAs(mTypeMapper));
            Assert.That(mInjeCtor.MappingProvider, Is.SameAs(mInjeCtor.Mapper));
        }

        [Test]
        public void MappingProvider_SetMappingProvider_ThrowsInvalidOperationException()
        {
            Assert.Throws<InvalidOperationException>(() => mInjeCtor.MappingProvider = null);
        }

        [Test]
        public void CreateScope_GetScope_NewScopeCreated()
        {
            var scope = mInjeCtor.CreateScope();

            Assert.That(scope, Is.Not.Null);
            Assert.That(scope.Creator, Is.Not.Null);
            Assert.That(scope.Creator, Is.SameAs(mCreator));
            Assert.That(scope.TypeInformationProvider, Is.Not.Null);
            Assert.That(scope.TypeInformationProvider, Is.SameAs(mTypeInformationProvider));
            Assert.That(scope.MappingProvider, Is.Not.Null);
            Assert.That(scope.MappingProvider, Is.SameAs(mTypeMapper));
            Assert.That(DummyScope.CreationCounter, Is.EqualTo(2));
        }

        [Test]
        public void Dispose_MultipleScopes_ScopesDisposed()
        {
            mInjeCtor.CreateScope();
            mInjeCtor.CreateScope();
            mInjeCtor.CreateScope();

            mInjeCtor.Dispose();

            Assert.That(DummyScope.CreationCounter, Is.EqualTo(4)); // 3  + 1 for default scope
            Assert.That(DummyScope.DisposeCounter, Is.EqualTo(4)); // same as above
        }

        [Test]
        public void Dispose_SingletonWithAndWithoutDispose_DisposeCorrect()
        {
            var disposeSingleton = mInjeCtor.Create<BaseClassForSingleton>();
            var nonDisposeSingleton = mInjeCtor.Create<BaseClassForNonDisposableSingleton>();

            Assert.That(disposeSingleton, Is.Not.Null);
            Assert.That(nonDisposeSingleton, Is.Not.Null);
            Assert.That(disposeSingleton, Is.Not.SameAs(nonDisposeSingleton));
            Assert.That(disposeSingleton, Is.InstanceOf<BaseClassForSingleton>());
            Assert.That(disposeSingleton, Is.InstanceOf<SingletonClass>());
            Assert.That(nonDisposeSingleton, Is.InstanceOf<BaseClassForNonDisposableSingleton>());
            Assert.That(nonDisposeSingleton, Is.InstanceOf<NonDisposableSingleton>());

            mInjeCtor.Dispose();

            Assert.That(nonDisposeSingleton.IsDisposed, Is.False);
            Assert.That(disposeSingleton.IsDisposed, Is.True);
        }

        [Test]
        public void SetSingletons_Call_ThrowsInvalidOperationException()
        {
            Assert.Throws<InvalidOperationException>(() => mInjeCtor.SetSingletons(null));
        }

        [Test]
        public void AddTypeMapping_AddTypeInformation_TypeInformationAdded()
        {
            Assert.That(mTypeInformationProvider.AddedTypes.Count, Is.EqualTo(0));

            mInjeCtor.Mapper.Add<Calculator>();

            Assert.That(mTypeInformationProvider.AddedTypes.Count, Is.EqualTo(1));
            Assert.That(mTypeInformationProvider.AddedTypes, Contains.Item(typeof(Calculator)));
        }

        [TestCase(false)]
        [TestCase(true)]
        public void Create_SingletonInstance_TypeInformationSetDependingOnExistingTypeInformation(bool addTypeInformation)
        {
            mTypeMapper.SetCreationInstruction(CreationInstruction.Singleton);

            if (addTypeInformation)
            {
                mTypeInformationProvider.SetTypeInformations(new Dictionary<Type, Dictionary<Type, List<PropertyInfo>>>
                {
                    { typeof(Calculator),
                        new Dictionary<Type, List<PropertyInfo>>
                        {
                            { typeof(IGreeter),
                                typeof(Calculator).GetProperties().Where(p => p.Name == nameof(Calculator.Greeter)).ToList()
                            }
                        }
                    }
                });
            }

            var createdObject = mInjeCtor.Create<ICalculator>();

            Assert.That(createdObject, Is.Not.Null);
            Assert.That(createdObject, Is.InstanceOf<Calculator>());

            Calculator calc = (Calculator)createdObject;

            if (addTypeInformation)
            {
                Assert.That(calc.Greeter, Is.Not.Null);
                Assert.That(calc.Greeter, Is.InstanceOf<Greeter>());
            }
            else
            {
                Assert.That(calc.Greeter, Is.Null);
            }
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

        #endregion
    }
}
