using InjeCtor.Core.Registration;
using InjeCtor.Core.Test.Interfaces;
using InjeCtor.Core.Test.RuntimeLoading;
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
            mTypeMapper = new DynamicTypeMapper();
            mLoadContext = new TestAssemblyLoadContext();
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
            Assert.IsNotNull(mapping);
            Assert.AreEqual(typeof(Calculator), mapping.MappedType);
            Assert.AreEqual(CreationInstruction.Always, mapping.CreationInstruction);
        }

        [Test]
        public void AddDynamicMapping_AppDomain_ResolveOnMappingAdd_Successfull()
        {
            Assert.IsTrue(mTypeMapper.Add<ICalculator>().Resolve());

            ITypeMapping? mapping = mTypeMapper.GetTypeMapping<ICalculator>();
            Assert.IsNotNull(mapping);
            Assert.AreEqual(typeof(Calculator), mapping.MappedType);
            Assert.AreEqual(CreationInstruction.Always, mapping.CreationInstruction);
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
            Assert.AreEqual(typeof(Calculator), mappings[0].MappedType);
            Assert.AreEqual(CreationInstruction.Scope, mappings[0].CreationInstruction);
            Assert.AreEqual(typeof(Greeter), mappings[1].MappedType);
            Assert.AreEqual(CreationInstruction.Singleton, mappings[1].CreationInstruction);
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
            Assert.AreEqual(typeof(Calculator), mappings[0].MappedType);
            Assert.AreEqual(CreationInstruction.Scope, mappings[0].CreationInstruction);
            Assert.AreEqual(typeof(Greeter), mappings[1].MappedType);
            Assert.AreEqual(CreationInstruction.Singleton, mappings[1].CreationInstruction);
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

        #endregion

        #region Private Methods

        private string GetFullPathName(string nameOfAssembly)
        {
            string? path = Path.GetDirectoryName(GetType().Assembly.Location);

            path = Path.Combine(path, $"{nameOfAssembly}.dll");
            return path;
        }

        #endregion
    }
}
