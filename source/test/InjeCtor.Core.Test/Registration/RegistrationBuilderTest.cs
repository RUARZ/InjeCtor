using InjeCtor.Core.Registration;
using InjeCtor.Core.Test.Interfaces;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace InjeCtor.Core.Test.Registration
{
    [TestFixture]
    public class RegistrationBuilderTest
    {
        #region Private Fields

        private IRegistrationBuilder mRegistrationBuilder;

        #endregion

        #region SetUp / TearDown

        [SetUp]
        public void SetUp()
        {
            mRegistrationBuilder = new RegistrationBuilder();
        }

        #endregion

        #region Tests

        [Test]
        public void RegistrationsAdd_Successfull()
        {
            Assert.IsFalse(mRegistrationBuilder.IsBuild);

            mRegistrationBuilder.Add<ICalculator>().As<Calculator>();
            mRegistrationBuilder.Add<IGreeter>().As<Greeter>();

            mRegistrationBuilder.Build();

            Assert.IsTrue(mRegistrationBuilder.IsBuild);

            IReadOnlyList<IRegisteredItem> items = mRegistrationBuilder.GetRegisteredItems();

            Assert.AreEqual(2, items.Count);
            Assert.AreEqual(typeof(ICalculator), items[0].RegisteredType);
            Assert.AreEqual(typeof(Calculator), items[0].TargetType);
            Assert.AreEqual(typeof(IGreeter), items[1].RegisteredType);
            Assert.AreEqual(typeof(Greeter), items[1].TargetType);
        }

        [Test]
        public void RegistrationsAdd_DoubleRegistration()
        {
            Assert.IsFalse(mRegistrationBuilder.IsBuild);

            mRegistrationBuilder.Add<ICalculator>().As<Calculator>();

            Assert.Throws<InvalidOperationException>(() => mRegistrationBuilder.Add<ICalculator>());

            Assert.IsFalse(mRegistrationBuilder.IsBuild);
        }

        [Test]
        public void RegistrationsGetRegistration_Successfull()
        {
            Assert.IsFalse(mRegistrationBuilder.IsBuild);

            mRegistrationBuilder.Add<ICalculator>().As<Calculator>();
            mRegistrationBuilder.Add<IGreeter>().As<Greeter>();

            mRegistrationBuilder.Build();

            Assert.IsTrue(mRegistrationBuilder.IsBuild);

            var item = mRegistrationBuilder.GetRegistration<ICalculator>();

            Assert.IsNotNull(item);
            Assert.AreEqual(typeof(ICalculator), item.RegisteredType);
            Assert.AreEqual(typeof(Calculator), item.TargetType);
        }

        [Test]
        public void RegistrationsGetRegistration_NotFound()
        {
            Assert.IsFalse(mRegistrationBuilder.IsBuild);

            mRegistrationBuilder.Add<ICalculator>().As<Calculator>();

            mRegistrationBuilder.Build();

            Assert.IsTrue(mRegistrationBuilder.IsBuild);

            var item = mRegistrationBuilder.GetRegistration<IGreeter>();

            Assert.IsNull(item);
        }

        [Test]
        public void RegistrationsGetRegistration_InvalidOperation_RequestBeforeBuild()
        {
            Assert.IsFalse(mRegistrationBuilder.IsBuild);

            mRegistrationBuilder.Add<ICalculator>().As<Calculator>();

            Assert.Throws<InvalidOperationException>(() => mRegistrationBuilder.GetRegistration<ICalculator>());

            Assert.IsFalse(mRegistrationBuilder.IsBuild);
        }

        #endregion
    }
}
