using InjeCtor.Core.TypeMapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InjeCtor.Configuration.Test.TestImplementations
{
    internal class TestTypeMapper : ITypeMapper
    {
        public event EventHandler<MappingAddedEventArgs>? MappingAdded;

        private List<TypeMapping> mMappings = new List<TypeMapping>();

        public ITypeMappingBuilder<T> Add<T>()
        {
            TypeMapping<T> mapping = new TypeMapping<T>();
            mMappings.Add(mapping);
            return mapping;
        }

        public ITypeMappingBuilder Add(Type type)
        {
            TypeMapping mapping = new TypeMapping(type);
            mMappings.Add(mapping);
            return mapping;
        }

        public void AddScopeSingleton<T>() where T : class
        {
            Add<T>().AsScopeSingleton<T>();
        }

        public void AddScopeSingleton(Type type)
        {
            Add(type).AsScopeSingleton(type);
        }

        public void AddSingleton<T>() where T : class
        {
            Add<T>().AsSingleton<T>();
        }

        public void AddSingleton(Type type)
        {
            Add(type).AsSingleton(type);
        }

        public void AddSingleton<T>(T instance) where T : class
        {
            Add<T>().AsSingleton(instance);
        }

        public void AddTransient<T>() where T : class
        {
            Add<T>().As<T>();
        }

        public void AddTransient(Type type)
        {
            Add(type).As(type);
        }

        public ITypeMapping? GetTypeMapping<T>()
        {
            return mMappings.FirstOrDefault(x => x.SourceType == typeof(T));
        }

        public ITypeMapping? GetTypeMapping(Type type)
        {
            return mMappings.FirstOrDefault(x => x.SourceType == type);
        }

        public IReadOnlyList<ITypeMapping> GetTypeMappings()
        {
            return mMappings;
        }
    }
}
