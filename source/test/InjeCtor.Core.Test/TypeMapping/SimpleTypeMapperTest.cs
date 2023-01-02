using InjeCtor.Core.Test.Interfaces;
using InjeCtor.Core.TypeMapping;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

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
            
            ITypeMapping? mapping = items.FirstOrDefault(x => x.SourceType == typeof(ICalculator));
            AssertTypeMapping(mapping, typeof(ICalculator), typeof(Calculator), CreationInstruction.Always);
            mapping = items.FirstOrDefault(x => x.SourceType == typeof(IGreeter));
            AssertTypeMapping(mapping, typeof(IGreeter), typeof(Greeter), CreationInstruction.Always);
        }

        [Test]
        public void Add_WithType_Successfull()
        {
            mTypeMapper.Add(typeof(ICalculator)).As(typeof(Calculator));
            mTypeMapper.Add(typeof(IGreeter)).As(typeof(Greeter));

            IReadOnlyList<ITypeMapping> items = mTypeMapper.GetTypeMappings();

            Assert.AreEqual(2, items.Count);

            ITypeMapping? mapping = items.FirstOrDefault(x => x.SourceType == typeof(ICalculator));
            AssertTypeMapping(mapping, typeof(ICalculator), typeof(Calculator), CreationInstruction.Always);
            mapping = items.FirstOrDefault(x => x.SourceType == typeof(IGreeter));
            AssertTypeMapping(mapping, typeof(IGreeter), typeof(Greeter), CreationInstruction.Always);
        }

        [Test]
        public void Add_WithType_InvalidMappedType()
        {
            Assert.Throws<InvalidOperationException>(() => mTypeMapper.Add(typeof(ICalculator)).As(typeof(Greeter)));
        }

        [Test]
        public void Add_MappingAdded_ThrownAfterCompletion()
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

            mapping.As<Calculator>();

            Assert.That(eventCounter, Is.EqualTo(1));
            Assert.That(args, Is.Not.Null);
            Assert.That(args.Mapping.SourceType, Is.EqualTo(typeof(ICalculator)));
            Assert.That(args.Mapping.MappedType, Is.EqualTo(typeof(Calculator)));
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

            AssertTypeMapping(item, typeof(ICalculator), typeof(Calculator), CreationInstruction.Always);

            item = mTypeMapper.GetTypeMapping(typeof(ICalculator));

            AssertTypeMapping(item, typeof(ICalculator), typeof(Calculator), CreationInstruction.Always);
        }

        [Test]
        public void RegistrationsAdd_WithCreationInstructionChange_Successfull()
        {
            mTypeMapper.Add<ICalculator>().AsScopeSingleton<Calculator>();
            mTypeMapper.Add<IGreeter>().AsSingleton<Greeter>();

            IReadOnlyList<ITypeMapping> items = mTypeMapper.GetTypeMappings();

            Assert.AreEqual(2, items.Count);

            ITypeMapping? mapping = items.FirstOrDefault(x => x.SourceType == typeof(ICalculator));
            AssertTypeMapping(mapping, typeof(ICalculator), typeof(Calculator), CreationInstruction.Scope);
            mapping = items.FirstOrDefault(x => x.SourceType == typeof(IGreeter));
            AssertTypeMapping(mapping, typeof(IGreeter), typeof(Greeter), CreationInstruction.Singleton);
        }

        [Test]
        public void Add_WithPassedSingletonInstance_Successful()
        {
            Calculator calc = new Calculator();
            Greeter greeter = new Greeter();

            mTypeMapper.Add<ICalculator>().AsSingleton(calc);
            mTypeMapper.Add<IGreeter>().AsSingleton(greeter);

            IReadOnlyList<ITypeMapping> items = mTypeMapper.GetTypeMappings();

            Assert.AreEqual(2, items.Count);

            ITypeMapping? mapping = items.FirstOrDefault(x => x.SourceType == typeof(ICalculator));
            AssertTypeMapping(mapping, typeof(ICalculator), typeof(Calculator), CreationInstruction.Singleton, calc);
            mapping = items.FirstOrDefault(x => x.SourceType == typeof(IGreeter));
            AssertTypeMapping(mapping, typeof(IGreeter), typeof(Greeter), CreationInstruction.Singleton, greeter);
        }

        [Test]
        public void RegistrationsGetRegistration_NotFound()
        {
            mTypeMapper.Add<ICalculator>().As<Calculator>();

            var item = mTypeMapper.GetTypeMapping<IGreeter>();

            Assert.IsNull(item);
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
        public void Add_MappingOfClassDirectlyWithType_MappingAdded(CreationInstruction instruction)
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
