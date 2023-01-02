using InjeCtor.Core.Test.Interfaces;
using InjeCtor.Core.Test.RuntimeLoading;
using InjeCtor.Core.TypeMapping;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace InjeCtor.Core.Test.Registration
{
    [TestFixture]
    public class DynamicTypeMapperTest
    {
        #region Consts

        private const string ASSEMBLY_TO_LOAD_NAME = "InjeCtor.Core.Implementations";

        #endregion

        #region Private Fields

        private DynamicTypeMapper? mTypeMapper;
        private TestAssemblyLoadContext? mLoadContext;

        #endregion

        #region SetUp / TearDown

        [SetUp]
        public void SetUp()
        {
            mLoadContext = new TestAssemblyLoadContext();
            //for unit test purposes since loaded assemblies from the context remains stuck in app domain..
            mTypeMapper = new DynamicTypeMapper(() => 
                mLoadContext.Assemblies.Concat(new Assembly[] { GetType().Assembly }).ToArray());
        }

        [TearDown]
        public void TearDown()
        {
            mLoadContext?.Unload();
        }

        #endregion

        #region Tests

        [Test]
        public void AddDynamicMapping_AppDomain_Successfull()
        {
            mTypeMapper.Add<ICalculator>();

            Assert.IsTrue(mTypeMapper.Resolve());

            ITypeMapping? mapping = mTypeMapper.GetTypeMapping<ICalculator>();
            AssertTypeMapping(mapping, typeof(ICalculator), typeof(Calculator), CreationInstruction.Always);
        }

        [Test]
        public void AddDynamicMapping_WithType_AppDomain_Successfull()
        {
            mTypeMapper.Add(typeof(ICalculator));

            Assert.IsTrue(mTypeMapper.Resolve());

            ITypeMapping? mapping = mTypeMapper.GetTypeMapping<ICalculator>();
            AssertTypeMapping(mapping, typeof(ICalculator), typeof(Calculator), CreationInstruction.Always);
        }

        [Test]
        public void AddDynamicMapping_AppDomain_ResolveOnMappingAdd_Successfull()
        {
            Assert.IsTrue(mTypeMapper.Add<ICalculator>().Resolve());

            ITypeMapping? item = mTypeMapper.GetTypeMapping<ICalculator>();

            AssertTypeMapping(item, typeof(ICalculator), typeof(Calculator), CreationInstruction.Always);

            item = mTypeMapper.GetTypeMapping(typeof(ICalculator));

            AssertTypeMapping(item, typeof(ICalculator), typeof(Calculator), CreationInstruction.Always);
        }

        [Test]
        public void AddDynamicMapping_WithType_ResolveOnMappingAdd_Successfull()
        {
            Assert.IsTrue(mTypeMapper.Add(typeof(ICalculator)).Resolve(GetType().Assembly));

            ITypeMapping? item = mTypeMapper.GetTypeMapping<ICalculator>();

            AssertTypeMapping(item, typeof(ICalculator), typeof(Calculator), CreationInstruction.Always);

            item = mTypeMapper.GetTypeMapping(typeof(ICalculator));

            AssertTypeMapping(item, typeof(ICalculator), typeof(Calculator), CreationInstruction.Always);
        }

        [Test]
        public void Add_MappingAdded_RaisedAfterCompletion()
        {
            int eventCounter = 0;
            MappingAddedEventArgs? args = null;
            mTypeMapper.MappingAdded += (sender, e) =>
            {
                eventCounter++;
                args = e;
            };

            var mapping = mTypeMapper.Add<ICalculator>();

            Assert.That(eventCounter, Is.EqualTo(0));
            Assert.That(args, Is.Null);

            mapping.Resolve();

            Assert.That(eventCounter, Is.EqualTo(1));
            Assert.That(args, Is.Not.Null);
            Assert.That(args.Mapping.SourceType, Is.EqualTo(typeof(ICalculator)));
            Assert.That(args.Mapping.MappedType, Is.EqualTo(typeof(Calculator)));
        }

        [Test]
        public void AddMappingsAndSetCreationInstructions_Successfull()
        {
            mTypeMapper.Add<ICalculator>().AsScopeSingleton();
            mTypeMapper.Add<IGreeter>().AsSingleton();
            mTypeMapper.Resolve();

            IReadOnlyList<ITypeMapping>? mappings = mTypeMapper.GetTypeMappings();
            Assert.IsNotNull(mappings);
            Assert.AreEqual(2, mappings.Count);
            ITypeMapping? mapping = mappings.FirstOrDefault(x => x.SourceType == typeof(ICalculator));
            AssertTypeMapping(mapping, typeof(ICalculator), typeof(Calculator), CreationInstruction.Scope);
            mapping = mappings.FirstOrDefault(x => x.SourceType == typeof(IGreeter));
            AssertTypeMapping(mapping, typeof(IGreeter), typeof(Greeter), CreationInstruction.Singleton);
        }

        [Test]
        public void AddDirectMappingsAndSetCreationInstructions_Successfull()
        {
            mTypeMapper.Add<ICalculator>().As<Calculator>().AsScopeSingleton();
            mTypeMapper.Add<IGreeter>().As<Greeter>().AsSingleton();
            mTypeMapper.Resolve();

            IReadOnlyList<ITypeMapping>? mappings = mTypeMapper.GetTypeMappings();
            Assert.IsNotNull(mappings);
            Assert.AreEqual(2, mappings.Count);
            ITypeMapping? mapping = mappings.FirstOrDefault(x => x.SourceType == typeof(ICalculator));
            AssertTypeMapping(mapping, typeof(ICalculator), typeof(Calculator), CreationInstruction.Scope);
            mapping = mappings.FirstOrDefault(x => x.SourceType == typeof(IGreeter));
            AssertTypeMapping(mapping, typeof(IGreeter), typeof(Greeter), CreationInstruction.Singleton);
        }

        [Test]
        public void AddDirectMappingsWithSingletonInstances_Successfull()
        {
            Calculator calc = new Calculator();
            Greeter greeter = new Greeter();
            mTypeMapper.Add<ICalculator>().AsSingleton(calc);
            mTypeMapper.Add<IGreeter>().AsSingleton(greeter);

            IReadOnlyList<ITypeMapping>? mappings = mTypeMapper.GetTypeMappings();
            Assert.IsNotNull(mappings);
            Assert.AreEqual(2, mappings.Count);
            ITypeMapping? mapping = mappings.FirstOrDefault(x => x.SourceType == typeof(ICalculator));
            AssertTypeMapping(mapping, typeof(ICalculator), typeof(Calculator), CreationInstruction.Singleton, calc);
            mapping = mappings.FirstOrDefault(x => x.SourceType == typeof(IGreeter));
            AssertTypeMapping(mapping, typeof(IGreeter), typeof(Greeter), CreationInstruction.Singleton, greeter);
        }

        [Test]
        public void AddDynamicMapping_SpecificAssembly_Successfull()
        {
            string assemblyPath = GetFullPathName(ASSEMBLY_TO_LOAD_NAME);
            Assembly assembly = mLoadContext.LoadFromAssemblyPath(assemblyPath);

            Assert.IsNotNull(assembly, "The assembly could not be loaded!");

            mTypeMapper.Add<ICalculator>();

            Assert.IsTrue(mTypeMapper.Resolve(assembly));

            ITypeMapping? mapping = mTypeMapper.GetTypeMapping<ICalculator>();
            Assert.IsNotNull(mapping);
            Assert.AreNotEqual(typeof(Calculator), mapping.MappedType);
            Assert.AreEqual(CreationInstruction.Always, mapping.CreationInstruction);

            Assert.IsTrue(mapping.MappedType.Assembly.FullName.Contains(ASSEMBLY_TO_LOAD_NAME));
        }

        [Test]
        public void AddDynamicMapping_SpecificAssembly_ResolveOnMappingAdd_Successfull()
        {
            string assemblyPath = GetFullPathName(ASSEMBLY_TO_LOAD_NAME);
            Assembly assembly = mLoadContext.LoadFromAssemblyPath(assemblyPath);

            Assert.IsNotNull(assembly, "The assembly could not be loaded!");

            Assert.IsTrue(mTypeMapper.Add<ICalculator>().Resolve(assembly));

            ITypeMapping? mapping = mTypeMapper.GetTypeMapping<ICalculator>();
            Assert.IsNotNull(mapping);
            Assert.AreNotEqual(typeof(Calculator), mapping.MappedType);

            Assert.IsTrue(mapping.MappedType.Assembly.FullName.Contains(ASSEMBLY_TO_LOAD_NAME));
        }

        [Test]
        public void AddDynamicMapping_DuplicatePossibleImplementations_NoMappingPossible()
        {
            string assemblyPath = GetFullPathName(ASSEMBLY_TO_LOAD_NAME);
            Assembly assembly = mLoadContext.LoadFromAssemblyPath(assemblyPath);

            Assert.IsNotNull(assembly, "The assembly could not be loaded!");

            mTypeMapper.Add<ICalculator>();

            Assert.IsFalse(mTypeMapper.Resolve());

            ITypeMapping? mapping = mTypeMapper.GetTypeMapping<ICalculator>();
            Assert.IsNull(mapping);
        }

        [Test]
        public void AddDynamicMapping_DuplicatePossibleImplementations_ResolveOnMappingAdd_NoMappingPossible()
        {
            string assemblyPath = GetFullPathName(ASSEMBLY_TO_LOAD_NAME);
            Assembly assembly = mLoadContext.LoadFromAssemblyPath(assemblyPath);

            Assert.IsNotNull(assembly, "The assembly could not be loaded!");

            Assert.IsFalse(mTypeMapper.Add<ICalculator>().Resolve());

            ITypeMapping? mapping = mTypeMapper.GetTypeMapping<ICalculator>();
            Assert.IsNull(mapping);
        }

        [TestCase(CreationInstruction.Always)]
        [TestCase(CreationInstruction.Scope)]
        [TestCase(CreationInstruction.Singleton)]
        public void Add_MappingOfClassDirectly_MappingAdded(CreationInstruction instruction)
        {
            switch (instruction)
            {
                case CreationInstruction.Always:
                    mTypeMapper.AddTransient<Calculator>();
                    break;
                case CreationInstruction.Scope:
                    mTypeMapper.AddScopeSingleton<Calculator>();
                    break;
                case CreationInstruction.Singleton:
                    mTypeMapper.AddSingleton<Calculator>();
                    break;
            }

            var item = mTypeMapper.GetTypeMapping<Calculator>();

            AssertTypeMapping(item, typeof(Calculator), typeof(Calculator), instruction, null);
        }

        [TestCase(CreationInstruction.Always)]
        [TestCase(CreationInstruction.Scope)]
        [TestCase(CreationInstruction.Singleton)]
        public void Add_MappingOfClassDirectly_WithType_MappingAdded(CreationInstruction instruction)
        {
            switch (instruction)
            {
                case CreationInstruction.Always:
                    mTypeMapper.AddTransient(typeof(Calculator));
                    break;
                case CreationInstruction.Scope:
                    mTypeMapper.AddScopeSingleton(typeof(Calculator));
                    break;
                case CreationInstruction.Singleton:
                    mTypeMapper.AddSingleton(typeof(Calculator));
                    break;
            }

            var item = mTypeMapper.GetTypeMapping<Calculator>();

            AssertTypeMapping(item, typeof(Calculator), typeof(Calculator), instruction, null);
        }

        [Test]
        public void Add_MappingOfClassDirectlyWithSingletonInstance_MappingAdded()
        {
            Calculator calc = new Calculator();

            mTypeMapper.AddSingleton(calc);

            var item = mTypeMapper.GetTypeMapping<Calculator>();

            AssertTypeMapping(item, typeof(Calculator), typeof(Calculator), CreationInstruction.Singleton, calc);
        }

        #endregion

        #region Private Methods

        private string GetFullPathName(string nameOfAssembly)
        {
            string? path = Path.GetDirectoryName(GetType().Assembly.Location);

            path = Path.Combine(path, $"{nameOfAssembly}.dll");
            return path;
        }

        private void AssertTypeMapping(ITypeMapping mapping, Type sourceType, Type mappedType, CreationInstruction creationInstruction, object? instance = null)
        {
            Assert.That(mapping, Is.Not.Null);
            Assert.That(mapping.SourceType, Is.EqualTo(sourceType));
            Assert.That(mapping.MappedType, Is.EqualTo(mappedType));
            Assert.That(mapping.CreationInstruction, Is.EqualTo(creationInstruction));

            if (instance == null)
                Assert.That(mapping.Instance, Is.Null);
            else
                Assert.That(mapping.Instance, Is.SameAs(instance));
        }

        #endregion
    }
}
