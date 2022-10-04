using InjeCtor.Core.Creation;
using InjeCtor.Core.Registration;
using InjeCtor.Core.Test.Interfaces;
using InjeCtor.Core.Test.TestClasses;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
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
        }

        #endregion

        #region Public Methods

        [Test]
        public void SimpleCreation_Success()
        {
            ICalculator calc = mCreator.Create<ICalculator>();
            Assert.IsNotNull(calc);
            Assert.IsInstanceOf<Calculator>(calc);

            Calculator calc2 = mCreator.Create<Calculator>();
            Assert.IsNotNull(calc2);
            Assert.IsInstanceOf<Calculator>(calc2);
            Assert.AreNotSame(calc, calc2);

            object calc3 = mCreator.Create(typeof(ICalculator));
            Assert.IsNotNull(calc3);
            Assert.IsInstanceOf<Calculator>(calc3);
            Assert.AreNotSame(calc3, calc2);
            Assert.AreNotSame(calc3, calc);

            object calc4 = mCreator.Create(typeof(Calculator));
            Assert.IsNotNull(calc4);
            Assert.IsInstanceOf<Calculator>(calc4);
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
            Assert.IsNotNull(obj.Greeter);
            Assert.IsInstanceOf<Calculator>(obj.Calculator);
            Assert.IsInstanceOf<Greeter>(obj.Greeter);
            Assert.IsNull(obj.Obj);
            Assert.AreEqual(13, obj.Number);
            Assert.IsNull(obj.Bit);
        }

        #endregion
    }
}
