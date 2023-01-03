using InjeCtor.Configuration.Xml.Validation;
using InjeCtor.Core;
using InjeCtor.Core.Builder;
using InjeCtor.Core.Creation;
using InjeCtor.Core.TypeInformation;
using InjeCtor.Core.TypeMapping;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace InjeCtor.Configuration.Xml
{
    /// <summary>
    /// Implements <see cref="IInjeCtorBuilder"/> to build <see cref="IInjeCtor"/> from xml configurations.
    /// </summary>
    public class XmlInjeCtorBuilder : IInjeCtorBuilder
    {
        #region Consts

        protected const string XML_SCHEMA_RESOURCE_NAME = "InjeCtor.Configuration.Resources.XmlConfigurationSchema.xsd";
        protected const string XML_NAMESPACE = "http://InjeCtor/Configuration/Xml";

        protected const string ROOT_ELEMENT_NAME = "Config";

        protected const string INJECTOR_CONFIGURATION_ELEMENT_NAME = "InjeCtor";
        protected const string TYPE_MAPPER_CONFIGURATION_ELEMENT_NAME = "TypeMapper";
        protected const string TYPE_INFORMATION_BUILDER_CONFIGURATION_ELEMENT_NAME = "TypeInformationBuilder";
        protected const string CREATOR_CONFIGURATION_ELEMENT_NAME = "Creator";
        protected const string USE_DYNAMIC_TYPE_MAPPER_ATTRIBUTE_NAME = "UseDynamicTypeMapper";

        protected const string MAPPINGS_ELEMENT_NAME = "Mappings";
        protected const string ALWAYS_CREATION_INSTRUCTION_ELEMENT_NAME = "Always";
        protected const string SCOPE_SINGLETON_CREATION_INSTRUCTION_ELEMENT_NAME = "ScopeSingleton";
        protected const string SINGLETON_CREATION_INSTRUCTION_ELEMENT_NAME = "Singleton";

        protected const string MAPPING_ELEMENT_NAME = "Mapping";
        protected const string SOURCE_TYPE_ATTRIBUTE_NAME = "SourceType";
        protected const string MAPPING_TYPE_ATTRIBUTE_NAME = "MappingType";
        protected const string DIRECT_TYPE_ATTRIBUTE_NAME = "DirectType";

        #endregion

        #region Fields

        protected readonly XmlValidator mValidator;
        protected readonly XNamespace mNamespace;
        protected ITypeMapper? mTypeMapper;
        protected ITypeInformationBuilder? mTypeInformationBuilder;
        protected IScopeAwareCreator? mScopeAwareCreator;
        protected IInjeCtor? mInjeCtor;

        #endregion

        #region Constructor

        /// <summary>
        /// Creates a new instance of <see cref="XmlInjeCtorBuilder"/> to build <see cref="IInjeCtor"/> instance from xml file.
        /// </summary>
        public XmlInjeCtorBuilder()
        {
            mValidator = new XmlValidator(typeof(XmlInjeCtorBuilder).Assembly.GetManifestResourceStream(XML_SCHEMA_RESOURCE_NAME));
            mNamespace = XNamespace.Get(XML_NAMESPACE);
        }

        #endregion

        #region Public Methods

        public void ParseFile(string fileName)
        {
            if (!File.Exists(fileName))
                throw new FileNotFoundException(fileName);

            using (FileStream fileStream = File.OpenRead(fileName))
            {
                ParseStream(fileStream);
            }
        }

        public void ParseStream(Stream stream)
        {
            XDocument doc = XDocument.Load(stream);

            ValidateDocument(doc);

            ParseDocument(doc);
        }

        #endregion

        #region IInjeCtorBuilder

        /// <inheritdoc/>
        public IInjeCtor Build()
        {
            if (mInjeCtor != null)
                return mInjeCtor;

            if (mTypeMapper is null || mScopeAwareCreator is null)
                throw new InvalidOperationException($"TypeMapper or creator is null! Call {nameof(ParseFile)} or {nameof(ParseStream)} to parse xml data before calling {nameof(Build)}!");

            if (mTypeMapper is IDynamicTypeMapper dynamicTypeMapper)
                dynamicTypeMapper.Resolve();

            if (mTypeInformationBuilder is ITypeInformationProvider typeInformationProvider)
                mInjeCtor = new Core.InjeCtor(mTypeMapper, typeInformationProvider, mScopeAwareCreator);
            else
                mInjeCtor = new Core.InjeCtor(mTypeMapper, new TypeInformationBuilder(), mScopeAwareCreator);

            return mInjeCtor;
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Validates the passed <paramref name="doc"/> and throws a <see cref="InvalidXmlConfigurationException"/> if the doc is not valid.
        /// </summary>
        /// <param name="doc">The <see cref="XDocument"/> to validate.</param>
        protected void ValidateDocument(XDocument doc)
        {
            List<string> validationErrors = mValidator.Validate(doc);

            if (validationErrors is null || !validationErrors.Any())
                return;

            StringBuilder builder = new StringBuilder();
            builder.AppendLine("The passed xml document is not valid!");

            foreach (string error in validationErrors)
                builder.AppendLine(error);

            throw new InvalidXmlConfigurationException(builder.ToString());
        }

        /// <summary>
        /// Parses the passed <paramref name="doc"/> and creates the data.
        /// </summary>
        /// <param name="doc">The <see cref="XDocument"/> to parse.</param>
        protected virtual void ParseDocument(XDocument doc)
        {
            XElement rootElement = doc.Element(mNamespace + ROOT_ELEMENT_NAME);
            XElement injeCtorElement = rootElement.Element(mNamespace + INJECTOR_CONFIGURATION_ELEMENT_NAME);
            mTypeMapper = CreateTypeMapper(injeCtorElement);
            mTypeInformationBuilder = CreateInstance<ITypeInformationBuilder, TypeInformationBuilder>(injeCtorElement, TYPE_INFORMATION_BUILDER_CONFIGURATION_ELEMENT_NAME);
            mScopeAwareCreator = CreateInstance<IScopeAwareCreator, DefaultCreator>(injeCtorElement, CREATOR_CONFIGURATION_ELEMENT_NAME);

            XElement mappingsElement = rootElement.Element(mNamespace + MAPPINGS_ELEMENT_NAME);

            AddMappings(mappingsElement, mTypeMapper, mTypeInformationBuilder);
        }

        protected virtual void AddMappings(XElement mappingsElement, ITypeMapper mapper, ITypeInformationBuilder builder)
        {
            if (mappingsElement == null)
                return;

            AddMappingsForCreationInstruction(mappingsElement, ALWAYS_CREATION_INSTRUCTION_ELEMENT_NAME, CreationInstruction.Always, builder, mapper);
            AddMappingsForCreationInstruction(mappingsElement, SCOPE_SINGLETON_CREATION_INSTRUCTION_ELEMENT_NAME, CreationInstruction.Scope, builder, mapper);
            AddMappingsForCreationInstruction(mappingsElement, SINGLETON_CREATION_INSTRUCTION_ELEMENT_NAME, CreationInstruction.Singleton, builder, mapper);
        }

        protected virtual void AddMappingsForCreationInstruction(XElement mappingsElement, string creationInstructionElementName, CreationInstruction creationInstruction, ITypeInformationBuilder builder, ITypeMapper mapper)
        {
            XElement? element = mappingsElement?.Element(mNamespace + creationInstructionElementName);

            if (element is null)
                return;

            foreach (XElement mapping in element.Elements(mNamespace + MAPPING_ELEMENT_NAME))
            {
                Type? sourceType = GetTypeFromAttribute(mapping, SOURCE_TYPE_ATTRIBUTE_NAME);

                if (sourceType == null)
                    continue;

                Type? mappingType = GetTypeFromAttribute(mapping, MAPPING_TYPE_ATTRIBUTE_NAME);
                bool directMapping = GetBoolAttribute(mapping, DIRECT_TYPE_ATTRIBUTE_NAME);

                if (directMapping)
                {
                    switch (creationInstruction)
                    {
                        case CreationInstruction.Always:
                            mapper.AddTransient(sourceType);
                            break;
                        case CreationInstruction.Scope:
                            mapper.AddScopeSingleton(sourceType);
                            break;
                        case CreationInstruction.Singleton:
                            mapper.AddSingleton(sourceType);
                            break;
                    }
                }
                else
                {
                    ITypeMappingBuilder? mappingBuilder = mapper.Add(sourceType);

                    if (mappingType is null)
                    {
                        if (creationInstruction == CreationInstruction.Always ||
                            !(mappingBuilder is IDynamicTypeMappingBuilder dynamicMappingBuilder))
                            continue;

                        switch (creationInstruction)
                        {
                            case CreationInstruction.Scope:
                                dynamicMappingBuilder.AsScopeSingleton();
                                break;
                            case CreationInstruction.Singleton:
                                dynamicMappingBuilder.AsSingleton();
                                break;
                        }

                        continue;
                    }

                    switch (creationInstruction)
                    {
                        case CreationInstruction.Always:
                            mappingBuilder.As(mappingType);
                            break;
                        case CreationInstruction.Scope:
                            mappingBuilder.AsScopeSingleton(mappingType);
                            break;
                        case CreationInstruction.Singleton:
                            mappingBuilder.AsSingleton(mappingType);
                            break;
                    }
                }
            }
        }

        private Type? GetTypeFromAttribute(XElement element, string attributeName)
        {
            if (element is null)
                return null;

            XAttribute attribute = element.Attribute(attributeName);

            if (attribute is null)
                return null;

            string typeName = attribute.Value;
            Type type = Type.GetType(typeName);
            return type;
        }

        private bool GetBoolAttribute(XElement element, string attributeName, bool fallBackValue = default(bool))
        {
            if (element is null)
                return fallBackValue;

            XAttribute attribute = element.Attribute(attributeName);

            if (attribute is null || !bool.TryParse(attribute.Value, out bool parsedValue))
                return fallBackValue;

            return parsedValue;
        }

        /// <summary>
        /// Creates the <see cref="ITypeMapper"/> to use from the <paramref name="element"/> for injector configuration.
        /// </summary>
        /// <param name="element">The <see cref="XElement"/> to get the information which <see cref="ITypeMapper"/> to use.</param>
        /// <returns>The <see cref="ITypeMapper"/> instance to use.</returns>
        protected virtual ITypeMapper CreateTypeMapper(XElement element)
        {
            if (element != null)
            {
                XElement typeMapperElement = element.Element(mNamespace + TYPE_MAPPER_CONFIGURATION_ELEMENT_NAME);

                if (typeMapperElement != null)
                {
                    ITypeMapper mapper = CreateInstance<ITypeMapper>(typeMapperElement.Value);
                    return mapper;
                }

                XAttribute? attribute = element.Attribute(USE_DYNAMIC_TYPE_MAPPER_ATTRIBUTE_NAME);

                if (attribute != null && bool.TryParse(attribute.Value, out bool useDynamicMapper) && useDynamicMapper)
                    return new DynamicTypeMapper();
            }

            return new SimpleTypeMapper();
        }

        /// <summary>
        /// Creates a instance to use for the passed type <typeparamref name="T"/>. Depending on the passed <paramref name="element"/> and
        /// a optional used sub element with the name <paramref name="elementName"/>.
        /// </summary>
        /// <typeparam name="T">The <see cref="Type"/> to return / create the instance for.</typeparam>
        /// <typeparam name="TFallback">The <see cref="Type"/> to use as fallback if the <paramref name="elementName"/> is not present.</typeparam>
        /// <param name="element">The <see cref="XElement"/> to check for a sub element with the <paramref name="elementName"/>.</param>
        /// <param name="elementName">The name of the <see cref="XElement"/> to try get for creation of the instance.</param>
        /// <returns>The created instance of <typeparamref name="T"/>.</returns>
        protected virtual T CreateInstance<T, TFallback>(XElement element, string elementName) where TFallback : T, new()
        {
            if (element != null)
            {
                XElement subElement = element.Element(mNamespace + elementName);

                if (subElement != null)
                {
                    T builder = CreateInstance<T>(subElement.Value);
                    return builder;
                }
            }

            return new TFallback();
        }

        /// <summary>
        /// Creates a new instance for the passed <paramref name="typeName"/>.
        /// </summary>
        /// <typeparam name="T">The type to return / cast it to.</typeparam>
        /// <param name="typeName">The name of the type to create.</param>
        /// <returns>The created type.</returns>
        protected T CreateInstance<T>(string typeName)
        {
            try
            {
                Type type = Type.GetType(typeName);

                object instance = Activator.CreateInstance(type);

                if (instance is T res)
                    return res;

                throw new InvalidCastException($"The created instance '{instance?.GetType()?.FullName ?? "<NULL>"}' is not of type '{typeof(T)}'!");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to create the type '{typeName}' or to cast it to '{typeof(T)}'!", ex);
            }
        }

        #endregion
    }
}
