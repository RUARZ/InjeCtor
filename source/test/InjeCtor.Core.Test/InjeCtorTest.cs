using InjeCtor.Core.Creation;
using InjeCtor.Core.Registration;
using InjeCtor.Core.Scope;
using InjeCtor.Core.Test.Interfaces;
using InjeCtor.Core.TypeInformation;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InjeCtor.Core.Test
{
    [TestFixture]
    public class InjeCtorTest
    {
        #region Private Fields

        private InjeCtor mInjeCtor;

        #endregion

        #region SetUp / TearDown

        [SetUp]
        public void SetUp()
        {

            mInjeCtor = new InjeCtor();
        }

        #endregion

        #region Tests

        #endregion

        #region DummyImplementations

        class DummyTypeMapping : ITypeMapping
        {
            public Type SourceType { get; set; }

            public Type? MappedType { get; set; }

            public CreationInstruction CreationInstruction { get; set; }
        }

        class DummyTypeMapper : ITypeMapper
        {
            public ITypeMapping<T> Add<T>()
            {
                throw new NotImplementedException();
            }

            public ITypeMapping? GetTypeMapping<T>()
            {
                return GetTypeMapping(typeof(T));
            }

            public ITypeMapping? GetTypeMapping(Type type)
            {
                DummyTypeMapping mapping = new DummyTypeMapping
                {
                    SourceType = type,
                };

                string typeName = type.FullName;
                if (typeName == typeof(ICalculator).FullName)
                    mapping.MappedType = typeof(Calculator);
                else if (typeName == typeof(IGreeter).FullName)
                    mapping.MappedType = typeof(Greeter);
                else if (typeName == typeof(IScope).FullName)
                    mapping.MappedType = typeof(DummyScope);

                return mapping;
            }

            public IReadOnlyList<ITypeMapping> GetTypeMappings()
            {
                throw new NotImplementedException();
            }
        }

        class DummyTypeInformationProvider : ITypeInformationProvider
        {
            public ITypeInformation? Get<T>()
            {
                return null;
            }

            public ITypeInformation? Get(Type type)
            {
                return null;
            }
        }

        class DummyCreator : IScopeAwareCreator
        {
            public ITypeMappingProvider? MappingProvider { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

            public object Create(Type type, IScope? scope)
            {
                string typeName = type.FullName;
                if (typeName == typeof(ICalculator).FullName)
                    return new Calculator();
                else if (typeName == typeof(IGreeter).FullName)
                    return new Greeter();
                else if (typeName == typeof(IScope).FullName)
                    return new DummyCreator();

                throw new InvalidOperationException($"unknown type '{typeName}'!");
            }

            public T Create<T>(IScope? scope)
            {
                return (T)Create(typeof(T), scope);
            }

            public object Create(Type type)
            {
                return Create(type, null);
            }

            public T Create<T>()
            {
                return (T)Create(typeof(T), null);
            }

            public void SetSingletons(IReadOnlyDictionary<Type, object> singletons)
            {
                
            }
        }

        class DummyScope : IScope
        {
            public ITypeInformationProvider? TypeInformationProvider { get; set; }
            public ICreator? Creator { get; set; }
            public ITypeMappingProvider? MappingProvider { get; set; }

            public object Create(Type type)
            {
                return Creator.Create(type);
            }

            public T Create<T>()
            {
                return Creator.Create<T>();
            }

            public void Dispose()
            {
                
            }

            public object? GetSingleton<T>()
            {
                return null;
            }

            public object? GetSingleton(Type type)
            {
                return null;
            }

            public void SetSingletons(IReadOnlyDictionary<Type, object> singletons)
            {
                
            }
        }

        #endregion
    }
}
