using InjeCtor.Core.Registration;
using InjeCtor.Core.Test.Interfaces;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace InjeCtor.Core.Test.Registration
{
    [TestFixture]
    public class SimpleTypeMapperTest
    {
        #region Private Fields

        private SimpleTypeMapper? mTypeMapper;

        #endregion

        #region SetUp / TearDown

        [SetUp]
        public void SetUp()
        {
            mTypeMapper = new SimpleTypeMapper();
        }

        #endregion

        #region Tests

        [Test]
        public void RegistrationsAdd_Successfull()
        {
            mTypeMapper.Add<ICalculator>().As<Calculator>();
            mTypeMapper.Add<IGreeter>().As<Greeter>();

            IReadOnlyList<ITypeMapping> items = mTypeMapper.GetTypeMappings();

            Assert.AreEqual(2, items.Count);
            Assert.AreEqual(typeof(ICalculator), items[0].SourceType);
            Assert.AreEqual(typeof(Calculator), items[0].MappedType);
            Assert.AreEqual(CreationInstruction.Always, items[0].CreationInstruction);
            Assert.AreEqual(typeof(IGreeter), items[1].SourceType);
            Assert.AreEqual(typeof(Greeter), items[1].MappedType);
            Assert.AreEqual(CreationInstruction.Always, items[1].CreationInstruction);
        }

        [Test]
        public void RegistrationsAdd_DoubleRegistration()
        {
            mTypeMapper.Add<ICalculator>().As<Calculator>();

            Assert.Throws<InvalidOperationException>(() => mTypeMapper.Add<ICalculator>());
        }

        [Test]
        public void RegistrationsGetRegistration_Successfull()
        {
            mTypeMapper.Add<ICalculator>().As<Calculator>();
            mTypeMapper.Add<IGreeter>().As<Greeter>();

            var item = mTypeMapper.GetTypeMapping<ICalculator>();

            Assert.IsNotNull(item);
            Assert.AreEqual(typeof(ICalculator), item.SourceType);
            Assert.AreEqual(typeof(Calculator), item.MappedType);
            Assert.AreEqual(CreationInstruction.Always, item.CreationInstruction);
        }

        [Test]
        public void RegistrationsAdd_WithCreationInstructionChange_Successfull()
        {
            mTypeMapper.Add<ICalculator>().As<Calculator>().AsScopeSingleton();
            mTypeMapper.Add<IGreeter>().As<Greeter>().AsSingleton();

            IReadOnlyList<ITypeMapping> items = mTypeMapper.GetTypeMappings();

            Assert.AreEqual(2, items.Count);
            Assert.AreEqual(typeof(ICalculator), items[0].SourceType);
            Assert.AreEqual(typeof(Calculator), items[0].MappedType);
            Assert.AreEqual(CreationInstruction.Scope, items[0].CreationInstruction);
            Assert.AreEqual(typeof(IGreeter), items[1].SourceType);
            Assert.AreEqual(typeof(Greeter), items[1].MappedType);
            Assert.AreEqual(CreationInstruction.Singleton, items[1].CreationInstruction);
        }

        [Test]
        public void RegistrationsGetRegistration_NotFound()
        {
            mTypeMapper.Add<ICalculator>().As<Calculator>();

            var item = mTypeMapper.GetTypeMapping<IGreeter>();

            Assert.IsNull(item);
        }

        #endregion
    }
}
