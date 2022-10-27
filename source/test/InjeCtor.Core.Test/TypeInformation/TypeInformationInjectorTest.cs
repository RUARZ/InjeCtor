using InjeCtor.Core.Test.Interfaces;
using InjeCtor.Core.Test.TestClasses;
using InjeCtor.Core.TypeInformation;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace InjeCtor.Core.Test.TypeInformation
{
    [TestFixture]
    public class TypeInformationInjectorTest
    {
        #region Private Fields

        private DummyTypeInformationProvider mTypeInformationProvider;
        private DummyCreator mCreator;
        private DummyScope mScope;
        private Calculator mCalculator;

        private TypeInformationInjector mInjector;

        #endregion

        #region SetUp / TearDown

        [SetUp]
        public void SetUp()
        {
            mTypeInformationProvider = new DummyTypeInformationProvider();
            mCreator = new DummyCreator();
            mScope = new DummyScope();

            mCalculator = new Calculator();

            mInjector = new TypeInformationInjector();
        }

        #endregion

        #region Tests

        [Test]
        public void InjectProperties_NullInstance_NoErrorOccurs()
        {
            mInjector.InjectProperties(null, mTypeInformationProvider, mCreator, mScope, null);
        }

        [Test]
        public void InjectProperties_NullTypeInformationProvider_NoErrorOccurs()
        {
            mInjector.InjectProperties(new Calculator(), null, mCreator, mScope, null);
        }

        [Test]
        public void InjectProperties_NullCreator_NoErrorOccurs()
        {
            mInjector.InjectProperties(new Calculator(), mTypeInformationProvider, null, mScope, null);
        }

        [Test]
        public void InjectProperties_NoTypeInformationFound_NoInstanceInjected()
        {
            mInjector.InjectProperties(mCalculator, mTypeInformationProvider, mCreator, mScope, null);

            Assert.That(mCalculator.Greeter, Is.Null);
        }

        [Test]
        public void InjectProperties_TypeInformationFound_PropertyInjected()
        {
            SetTypeInformationForCalculator();

            mInjector.InjectProperties(mCalculator, mTypeInformationProvider, mCreator, mScope, null);

            Assert.That(mCalculator.Greeter, Is.Not.Null);
            Assert.That(mCalculator.Greeter, Is.InstanceOf<Greeter>());
        }

        [Test]
        public void InjectProperties_TypeInformationFoundAndScopeNull_PropertyInjected()
        {
            SetTypeInformationForCalculator();

            mInjector.InjectProperties(mCalculator, mTypeInformationProvider, mCreator, null, null);

            Assert.That(mCalculator.Greeter, Is.Not.Null);
            Assert.That(mCalculator.Greeter, Is.InstanceOf<Greeter>());
        }

        #endregion

        #region Private Methods

        private void SetTypeInformationForCalculator()
        {
            mTypeInformationProvider.SetTypeInformations(new Dictionary<Type, Dictionary<Type, List<PropertyInfo>>>
                {
                    { typeof(Calculator),
                        new Dictionary<Type, List<PropertyInfo>>
                        {
                            { typeof(IGreeter),
                                typeof(Calculator).GetProperties().Where(p => p.Name == nameof(Calculator.Greeter)).ToList()
                            }
                        }
                    }
                });
        }

        #endregion
    }
}
