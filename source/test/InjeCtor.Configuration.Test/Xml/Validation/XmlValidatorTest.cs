using InjeCtor.Configuration.Xml.Validation;
using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;

namespace InjeCtor.Configuration.Test.Xml.Validation
{
    [TestFixture]
    public class XmlValidatorTest
    {
        #region Private Fields

        private XmlValidator mXmlValidator;

        #endregion

        #region SetUp / TearDown

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            Stream? stream = GetType().Assembly.GetManifestResourceStream("InjeCtor.Configuration.Test.Xml.Validation.SimpleTestSchema.xsd");
            mXmlValidator = new XmlValidator(stream);
        }

        #endregion

        #region Tests

        [Test]
        public void Validate_MissingElements_NotValid()
        {
            string xml = 
                $"<ElementList xmlns=\"http://test.com/SimpleTestSchema\">" +
                $"</ElementList>";

            XDocument doc = XDocument.Parse(xml);

            List<string>? errors = mXmlValidator.Validate(doc);

            Assert.That(errors, Is.Not.Null);
            Assert.That(errors, Is.Not.Empty);
            Assert.That(errors.Count, Is.EqualTo(1));
        }

        [Test]
        public void Validate_WrongElements_NotValid()
        {
            string xml =
                $"<ElementList xmlns=\"http://test.com/SimpleTestSchema\">" +
                $"  <Elementt>abc</Elementt>" +
                $"</ElementList>";

            XDocument doc = XDocument.Parse(xml);

            List<string>? errors = mXmlValidator.Validate(doc);

            Assert.That(errors, Is.Not.Null);
            Assert.That(errors, Is.Not.Empty);
            Assert.That(errors.Count, Is.EqualTo(1));
        }

        [Test]
        public void Validate_Valid()
        {
            string xml =
                $"<ElementList xmlns=\"http://test.com/SimpleTestSchema\">" +
                $"  <Element>abc</Element>" +
                $"</ElementList>";

            XDocument doc = XDocument.Parse(xml);

            List<string>? errors = mXmlValidator.Validate(doc);

            Assert.That(errors, Is.Not.Null);
            Assert.That(errors, Is.Empty);
            Assert.That(errors.Count, Is.EqualTo(0));
        }

        #endregion
    }
}
