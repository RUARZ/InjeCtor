using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Xml.Schema;

namespace InjeCtor.Configuration.Xml.Validation
{
    /// <summary>
    /// Class for validating the passed xml configuration
    /// </summary>
    public class XmlValidator
    {
        #region Private Fields

        private readonly XmlSchemaSet mSchemaSet;

        #endregion

        #region Constructor

        /// <summary>
        /// Creates a new instance of <see cref="XmlValidator"/> and set's its schemas to use for validation from <paramref name="streams"/>.
        /// </summary>
        /// <param name="stream">The streams with the xsd's to use for validation.</param>
        public XmlValidator(IEnumerable<Stream> streams)
            : this(streams.Select(s => XmlSchema.Read(s, null)))
        {

        }

        /// <summary>
        /// Creates a new instance of <see cref="XmlValidator"/> and set's its schema to use for validation from <paramref name="stream"/>.
        /// </summary>
        /// <param name="stream">The stream with the xsd to use for validation.</param>
        public XmlValidator(Stream stream)
            : this(new XmlSchema[] { XmlSchema.Read(stream, null) })
        {

        }

        /// <summary>
        /// Creates a new instance of <see cref="XmlValidator"/> and set's it's schema to use for validation.
        /// </summary>
        /// <param name="schema">The <see cref="XmlSchema"/> to use for validation.</param>
        public XmlValidator(XmlSchema schema)
            :this (new XmlSchema[] { schema })
        {
        }

        /// <summary>
        /// Creates a new instance of <see cref="XmlValidator"/> and set's it's schemas to use for validation.
        /// </summary>
        /// <param name="schema">The <see cref="XmlSchema"/>'s to use for validation.</param>
        public XmlValidator(IEnumerable<XmlSchema> schemas)
        {
            mSchemaSet = new XmlSchemaSet();

            foreach (XmlSchema schema in schemas)
                mSchemaSet.Add(schema);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Validates the passed <paramref name="doc"/> for correct xml schema.
        /// </summary>
        /// <param name="doc">The <see cref="XDocument"/> to validate.</param>
        /// <returns>A <see cref="List{T}"/> of <see cref="System.String"/> with the validation errors.</returns>
        public List<string> Validate(XDocument doc)
        {
            List<string> errors = new List<string>();

            doc.Validate(mSchemaSet, (s, e) =>
            {
                errors.Add(e.Message);
            });

            return errors;
        }

        #endregion
    }
}
