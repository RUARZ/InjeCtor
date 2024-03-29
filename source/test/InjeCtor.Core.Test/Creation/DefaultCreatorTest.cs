﻿using InjeCtor.Core.Creation;
using InjeCtor.Core.Exceptions;
using InjeCtor.Core.Invoke;
using InjeCtor.Core.Scope;
using InjeCtor.Core.Test.Interfaces;
using InjeCtor.Core.Test.TestClasses;
using InjeCtor.Core.TypeInformation;
using InjeCtor.Core.TypeMapping;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace InjeCtor.Core.Test.Creation
{
    [TestFixture]
    public class DefaultCreatorTest
    {
        #region Private Fields

        private SimpleTypeMapper mMappingProvider;
        private DefaultCreator mCreator;
        private DummyScope mDummyScope;

        #endregion

        #region SetUp / TearDown

        [SetUp]
        public void SetUp()
        {
            mMappingProvider = new SimpleTypeMapper();
            mMappingProvider.Add<ICalculator>().As<Calculator>();
            mMappingProvider.Add<IGreeter>().As<Greeter>();

            mCreator = new DefaultCreator();
            mCreator.MappingProvider = mMappingProvider;

            mDummyScope = new DummyScope();
            mDummyScope.Singletons.Add(typeof(ICalculator), new Calculator());
        }

        #endregion

        #region Public Methods

        [Test]
        public void SimpleCreation_Success()
        {
            ICalculator calc = mCreator.Create<ICalculator>();
            Assert.IsNotNull(calc);
            Assert.IsInstanceOf<Calculator>(calc);
            Assert.AreNotSame(mDummyScope.Singletons[typeof(ICalculator)], calc);

            Calculator calc2 = mCreator.Create<Calculator>();
            Assert.IsNotNull(calc2);
            Assert.IsInstanceOf<Calculator>(calc2);
            Assert.AreNotSame(mDummyScope.Singletons[typeof(ICalculator)], calc2);
            Assert.AreNotSame(calc, calc2);

            object calc3 = mCreator.Create(typeof(ICalculator));
            Assert.IsNotNull(calc3);
            Assert.AreNotSame(mDummyScope.Singletons[typeof(ICalculator)], calc3);
            Assert.IsInstanceOf<Calculator>(calc3);
            Assert.AreNotSame(calc3, calc2);
            Assert.AreNotSame(calc3, calc);

            object calc4 = mCreator.Create(typeof(Calculator));
            Assert.IsNotNull(calc4);
            Assert.IsInstanceOf<Calculator>(calc4);
            Assert.AreNotSame(mDummyScope.Singletons[typeof(ICalculator)], calc4);
            Assert.AreNotSame(calc4, calc3);
            Assert.AreNotSame(calc4, calc2);
            Assert.AreNotSame(calc4, calc);
        }

        [Test]
        public void CreateWithMostSuitableConstructor_And_AbstractTypeMapping_Success()
        {
            mMappingProvider.Add<AbractCreationBaseClass>().As<CreationClass>();

            CreationClass? obj = mCreator.Create<AbractCreationBaseClass>() as CreationClass;

            Assert.IsNotNull(obj);
            Assert.IsInstanceOf<AbractCreationBaseClass>(obj);
            Assert.IsNotNull(obj.Calculator);
            Assert.AreNotSame(mDummyScope.Singletons[typeof(ICalculator)], obj.Calculator);
            Assert.IsNotNull(obj.Greeter);
            Assert.IsInstanceOf<Calculator>(obj.Calculator);
            Assert.IsInstanceOf<Greeter>(obj.Greeter);
        }

        [Test]
        public void CreateWithMostSuitableConstructor_AbstractTypeMapping_And_DefaultParameterValues_Success()
        {
            mMappingProvider.Add<AbractCreationBaseClass>().As<CreationClassWithSomeDefaultPara>();

            CreationClassWithSomeDefaultPara? obj = mCreator.Create<AbractCreationBaseClass>() as CreationClassWithSomeDefaultPara;

            Assert.IsNotNull(obj);
            Assert.IsInstanceOf<CreationClassWithSomeDefaultPara>(obj);
            Assert.IsNotNull(obj.Calculator);
            Assert.AreNotSame(mDummyScope.Singletons[typeof(ICalculator)], obj.Calculator);
            Assert.IsNotNull(obj.Greeter);
            Assert.IsInstanceOf<Calculator>(obj.Calculator);
            Assert.IsInstanceOf<Greeter>(obj.Greeter);
            Assert.IsNotNull(obj.Obj);
            Assert.AreEqual(0, obj.Number);
            Assert.IsFalse(obj.Bit);
        }

        [Test]
        public void CreateWithMostSuitableConstructor_AbstractTypeMapping_And_DefaultParameterValuesWithNullables_Success()
        {
            mMappingProvider.Add<AbractCreationBaseClass>().As<CreationClassWithSomeDefaultParaAndNullable>();

            CreationClassWithSomeDefaultParaAndNullable? obj = mCreator.Create<AbractCreationBaseClass>() as CreationClassWithSomeDefaultParaAndNullable;

            Assert.IsNotNull(obj);
            Assert.IsInstanceOf<CreationClassWithSomeDefaultParaAndNullable>(obj);
            Assert.IsNotNull(obj.Calculator);
            Assert.AreNotSame(mDummyScope.Singletons[typeof(ICalculator)], obj.Calculator);
            Assert.IsNotNull(obj.Greeter);
            Assert.IsInstanceOf<Calculator>(obj.Calculator);
            Assert.IsInstanceOf<Greeter>(obj.Greeter);
            Assert.IsNull(obj.Obj);
            Assert.AreEqual(13, obj.Number);
            Assert.IsNull(obj.Bit);
        }

        [Test]
        public void CreationWithSingletonScope_Success()
        {
            mMappingProvider = new SimpleTypeMapper();
            mMappingProvider.Add<ICalculator>().AsSingleton<Calculator>();
            mMappingProvider.Add<IGreeter>().As<Greeter>();

            mCreator.MappingProvider = mMappingProvider;

            mMappingProvider.Add<AbractCreationBaseClass>().As<CreationClassWithSomeDefaultParaAndNullable>();

            CreationClassWithSomeDefaultParaAndNullable? obj = mCreator.Create<AbractCreationBaseClass>(mDummyScope) as CreationClassWithSomeDefaultParaAndNullable;

            Assert.IsNotNull(obj);
            Assert.IsInstanceOf<CreationClassWithSomeDefaultParaAndNullable>(obj);
            Assert.IsNotNull(obj.Calculator);
            Assert.IsNotNull(obj.Greeter);
            Assert.IsInstanceOf<Calculator>(obj.Calculator);
            Assert.AreSame(mDummyScope.Singletons[typeof(ICalculator)], obj.Calculator);
            Assert.IsInstanceOf<Greeter>(obj.Greeter);
            Assert.IsNull(obj.Obj);
            Assert.AreEqual(13, obj.Number);
            Assert.IsNull(obj.Bit);
        }

        [Test]
        public void Create_NotAddedTypeMappingClass_InstanceCreatedWithPassedParameters()
        {
            var createdObject = mCreator.Create<NotMappedClass>();

            Assert.That(createdObject, Is.Not.Null);
            Assert.That(createdObject.Calculator, Is.Not.Null);
            Assert.That(createdObject.Greeter, Is.Not.Null);
            Assert.That(createdObject.Calculator, Is.InstanceOf<Calculator>());
            Assert.That(createdObject.Greeter, Is.InstanceOf<Greeter>());
        }

        [Test]
        public void Create_NotMappedInterfaceAndAbstractClass_InvalidOperationException()
        {
            Assert.Throws<InvalidOperationException>(() => mCreator.Create<INotMappedInterface>());
            Assert.Throws<InvalidOperationException>(() => mCreator.Create<NotMappedAbstractClass>());
        }

        [Test]
        public void Create_SimpleCircularReference_CircularReferenceExceptionThrown()
        {
            mMappingProvider.Add<ICircularReferenceInterfaceA>().As<SimpleCircularReferenceClassA>();
            mMappingProvider.Add<ICircularReferenceInterfaceB>().As<SimpleCircularReferenceClassB>();

            Assert.Throws<CircularReferenceException>(() => mCreator.Create<ICircularReferenceInterfaceA>());
        }

        [Test]
        public void Create_CircularReference_CircularReferenceExceptionThrown()
        {
            mMappingProvider.Add<ICircularReferenceInterfaceA>().As<CircularReferenceClassA>();
            mMappingProvider.Add<ICircularReferenceInterfaceB>().As<CircularReferenceClassB>();
            mMappingProvider.Add<ICircularReferenceInterfaceC>().As<CircularReferenceClassC>();
            mMappingProvider.Add<ICircularReferenceInterfaceD>().As<CircularReferenceClassD>();

            Assert.Throws<CircularReferenceException>(() => mCreator.Create<ICircularReferenceInterfaceA>());
        }

        #endregion

        #region DummyScope

        class DummyScope : IScope
        {
            public Dictionary<Type, object> Singletons { get; } = new Dictionary<Type, object>();

            public ITypeMappingProvider? MappingProvider { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
            public ITypeInformationProvider? TypeInformationProvider { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
            public IScopeAwareCreator? Creator { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
            public IInvoker? Invoker { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

            public event EventHandler<RequestSingletonCreationEventArgs>? RequestSingletonCreationInstance;
            public event EventHandler? Disposing;

            public object Create(Type type, IScope? scope)
            {
                throw new NotImplementedException();
            }

            public T Get<T>(IScope? scope)
            {
                throw new NotImplementedException();
            }

            public object Get(Type type)
            {
                throw new NotImplementedException();
            }

            public T Get<T>()
            {
                throw new NotImplementedException();
            }

            public void Dispose()
            {
                throw new NotImplementedException();
            }

            public object? GetSingleton<T>()
            {
                return GetSingleton(typeof(T));
            }

            public object? GetSingleton(Type type)
            {
                if (Singletons.TryGetValue(type, out var singleton))
                    return singleton;

                return null;
            }

            public object? Invoke<TObj>(TObj obj, Expression<Func<TObj, Delegate>> expression, params object?[] parameters)
            {
                throw new NotImplementedException();
            }

            public object? Invoke(Expression<Func<Delegate>> expression, params object?[] parameters)
            {
                throw new NotImplementedException();
            }

            public void SetSingletons(IReadOnlyDictionary<Type, object> singletons)
            {
                throw new NotImplementedException();
            }
        }

        #endregion
    }
}
