using InjeCtor.Core.Test.TestClasses;
using InjeCtor.Core.TypeInformation;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InjeCtor.Core.Test.TypeInformation
{
    [TestFixture]
    public class TypeInformationBuilderTest
    {
        #region Private Fields

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        private TypeInformationBuilder mBuilder;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        #endregion

        #region SetUp / TearDown

        [SetUp]
        public void SetUp()
        {
            mBuilder = new TypeInformationBuilder();
        }

        #endregion

        #region Tests

        [Test]
        public void GetTypeWithoutTypeInformation()
        {
            Assert.IsNull(mBuilder.Get<SingleInjectProperty>());
        }

        [Test]
        public void AddTypeWithValidAttribute_Success()
        {
            mBuilder.Add<SingleInjectProperty>();

            ITypeInformation? info = mBuilder.Get<SingleInjectProperty>();

            Assert.IsNotNull(info);
            Assert.AreEqual(1, info.PropertiesToInject.Count);
            Assert.AreEqual(typeof(bool), info.PropertiesToInject.First().Key);
            Assert.AreEqual(1, info.PropertiesToInject.First().Value.Count);
            Assert.AreEqual(nameof(SingleInjectProperty.Inject), info.PropertiesToInject.First().Value.First().Name);
        }

        [Test]
        public void AddTypeWithNotValidAttribute_Success()
        {
            mBuilder.Add<NotValidInjectProperty>();

            ITypeInformation? info = mBuilder.Get<NotValidInjectProperty>();

            Assert.IsNotNull(info);
            Assert.AreEqual(0, info.PropertiesToInject.Count);
        }

        [Test]
        public void AddTypeWithMultipleValidAttribute_Success()
        {
            mBuilder.Add<MultipleInjectProperty>();

            ITypeInformation? info = mBuilder.Get<MultipleInjectProperty>();

            Assert.IsNotNull(info);
            Assert.AreEqual(3, info.PropertiesToInject.Count);
            Assert.IsTrue(info.PropertiesToInject.ContainsKey(typeof(bool)));
            Assert.IsTrue(info.PropertiesToInject.ContainsKey(typeof(int)));
            Assert.IsTrue(info.PropertiesToInject.ContainsKey(typeof(string)));
            Assert.AreEqual(1, info.PropertiesToInject[typeof(bool)].Count);
            Assert.AreEqual(1, info.PropertiesToInject[typeof(int)].Count);
            Assert.AreEqual(1, info.PropertiesToInject[typeof(string)].Count);
            Assert.AreEqual(nameof(MultipleInjectProperty.Inject1), info.PropertiesToInject[typeof(bool)].First().Name);
            Assert.AreEqual(nameof(MultipleInjectProperty.Inject2), info.PropertiesToInject[typeof(int)].First().Name);
            Assert.AreEqual(nameof(MultipleInjectProperty.Inject3), info.PropertiesToInject[typeof(string)].First().Name);
        }

        [Test]
        public void AddTypeWithMultipleValidAttributeAndMultipleSameTypes_Success()
        {
            mBuilder.Add<MultipleInjectSamePropertyTypes>();

            ITypeInformation? info = mBuilder.Get<MultipleInjectSamePropertyTypes>();

            Assert.IsNotNull(info);
            Assert.AreEqual(2, info.PropertiesToInject.Count);
            Assert.IsTrue(info.PropertiesToInject.ContainsKey(typeof(bool)));
            Assert.IsTrue(info.PropertiesToInject.ContainsKey(typeof(string)));
            Assert.AreEqual(3, info.PropertiesToInject[typeof(bool)].Count);
            Assert.AreEqual(2, info.PropertiesToInject[typeof(string)].Count);
            Assert.AreEqual(1, info.PropertiesToInject[typeof(bool)].Count(x => x.Name == nameof(MultipleInjectSamePropertyTypes.Inject1)));
            Assert.AreEqual(1, info.PropertiesToInject[typeof(bool)].Count(x => x.Name == nameof(MultipleInjectSamePropertyTypes.Inject2)));
            Assert.AreEqual(1, info.PropertiesToInject[typeof(bool)].Count(x => x.Name == nameof(MultipleInjectSamePropertyTypes.Inject3)));
            Assert.AreEqual(1, info.PropertiesToInject[typeof(string)].Count(x => x.Name == nameof(MultipleInjectSamePropertyTypes.Inject4)));
            Assert.AreEqual(1, info.PropertiesToInject[typeof(string)].Count(x => x.Name == nameof(MultipleInjectSamePropertyTypes.Inject5)));
        }

        #endregion
    }
}
