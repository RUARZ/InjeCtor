using InjeCtor.Core.Creation;
using InjeCtor.Core.Scope;
using InjeCtor.Core.TypeMapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InjeCtor.Configuration.Test.TestImplementations
{
    public class TestCreator : IScopeAwareCreator
    {
        public static int CreationCount { get; set; }

        public TestCreator()
        {
            CreationCount++;
        }

        public ITypeMappingProvider? MappingProvider { get; set; }

        public event EventHandler<RequestSingletonCreationEventArgs>? RequestSingletonCreationInstance;

        public object Create(Type type, IScope? scope, ICreationHistory? creationHistory = null)
        {
            throw new NotImplementedException();
        }

        public T Create<T>(IScope? scope, ICreationHistory? creationHistory = null)
        {
            throw new NotImplementedException();
        }

        public object Create(Type type)
        {
            throw new NotImplementedException();
        }

        public T Create<T>()
        {
            ITypeMapping? mapping = MappingProvider.GetTypeMapping(typeof(T));

            return (T)Activator.CreateInstance(mapping.MappedType);
        }

        public object CreateDirect(Type type, IScope? scope, ICreationHistory creationHistory)
        {
            throw new NotImplementedException();
        }

        public T CreateDirect<T>(IScope? scope, ICreationHistory creationHistory)
        {
            throw new NotImplementedException();
        }

        public void SetSingletons(IReadOnlyDictionary<Type, object> singletons)
        {
            
        }
    }
}
