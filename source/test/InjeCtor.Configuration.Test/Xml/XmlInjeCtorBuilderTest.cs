using InjeCtor.Configuration.Test.Interfaces;
using InjeCtor.Configuration.Test.TestImplementations;
using InjeCtor.Configuration.Xml;
using InjeCtor.Core;
using InjeCtor.Core.TypeInformation;
using InjeCtor.Core.TypeMapping;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace InjeCtor.Configuration.Test.Xml
{
    [TestFixture]
    public class XmlInjeCtorBuilderTest
    {
        #region Private Fields

        private string? mTestXmlFileName;
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        private XmlInjeCtorBuilder mBuilder;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        #endregion

        #region SetUp / TearDown

        [SetUp]
        public void SetUp()
        {
            mTestXmlFileName = Path.GetTempFileName();
            mBuilder = new XmlInjeCtorBuilder();
        }

        [TearDown]
        public void TearDown()
        {
            TestCreator.CreationCount = 0;

            if (File.Exists(mTestXmlFileName))
                File.Delete(mTestXmlFileName);
        }

        #endregion

        #region Tests

        [TestCase(false)]
        [TestCase(true)]
        public void ParseDocument_InvalidConfiguration(bool createFile)
        {
            #region Xml

            string xml = 
                "<Config xmlns=\"http://InjeCtor/Configuration/Xml\">" +
                    "<InjeCtor>" +
                        "<TypeMapper>InjeCtor.Configuration.Test.TestImplementations.TestTypeMapper, InjeCtor.Configuration.Test</TypeMapper>"+
                        "<TypeInformationBuilder>InjeCtor.Configuration.Test.TestImplementations.TestTypeInformationBuilder, InjeCtor.Configuration.Test</TypeInformationBuilder>"+
                        "<Creator>InjeCtor.Configuration.Test.TestImplementations.TestCreator, InjeCtor.Configuration.Test</Creator>"+
                    "</InjeCtor>" +
                    "<SomeInvalidElementHere></SomeInvalidElementHere>" +
                    "<Mappings>" +
                        "<Always>"+
                            "<Mapping SourceType=\"InjeCtor.Configuration.Test.Interfaces.IInterfaceA, InjeCtor.Configuration.Test\" MappingType=\"InjeCtor.Configuration.Test.Interfaces.ImplA, InjeCtor.Configuration.Test\" DirectType=\"false\"/>" +
                        "</Always>" +
                        "<ScopeSingleton>" +
                            "<Mapping SourceType=\"InjeCtor.Configuration.Test.Interfaces.IInterfaceB, InjeCtor.Configuration.Test\" MappingType=\"InjeCtor.Configuration.Test.Interfaces.ImplB, InjeCtor.Configuration.Test\" DirectType=\"false\"/>"+
                        "</ScopeSingleton>"+
                        "<Singleton>"+
                            "<Mapping SourceType=\"InjeCtor.Configuration.Test.Interfaces.ImplC, InjeCtor.Configuration.Test\" DirectType=\"true\"/>"+
                        "</Singleton>" +
                    "</Mappings>" +
                "</Config>";

            #endregion

            Assert.Throws<InvalidXmlConfigurationException>(() =>
            {
                if (createFile)
                {
                    File.WriteAllText(mTestXmlFileName, xml);
                    mBuilder.ParseFile(mTestXmlFileName);
                }
                else
                {
                    // passing string to a stream and parse the stream
                    ParseString(xml);
                }
            });
        }

        [TestCase(false)]
        [TestCase(true)]
        public void Build_WithSpecificImplementationsForInjeCtor_Success(bool createFile)
        {
            #region Xml Creation

            string xml =
                CreateXml
                (
                    CreateInjeCtorXml(typeof(TestTypeMapper), typeof(TestTypeInformationBuilder), typeof(TestCreator), null),
                    CreateAlwaysMappings(CreateMappingXml(typeof(IInterfaceA), typeof(ImplA), false)),
                    CreateScopeMappings(CreateMappingXml(typeof(IInterfaceB), typeof(ImplB), false)),
                    CreateSingletonMappings(CreateMappingXml(typeof(ImplC), null, true))
                );

            if (createFile)
            {
                File.WriteAllText(mTestXmlFileName, xml);
                mBuilder.ParseFile(mTestXmlFileName);
            }
            else
            {
                // passing string to a stream and parse the stream
                ParseString(xml);
            }

            #endregion

            IInjeCtor injeCtor = mBuilder.Build();

            Assert.That(injeCtor, Is.Not.Null);

            Assert.That(injeCtor.Mapper, Is.InstanceOf<TestTypeMapper>());
            Assert.That(injeCtor.TypeInformationProvider, Is.InstanceOf<TestTypeInformationBuilder>());
            Assert.That(injeCtor.TypeInformationBuilder, Is.InstanceOf<TestTypeInformationBuilder>());

            IReadOnlyList<ITypeMapping>? mappings = injeCtor.Mapper.GetTypeMappings();

            Assert.That(mappings, Is.Not.Null);
            Assert.That(mappings.Count, Is.EqualTo(6));

            ITypeMapping? typeMapping = mappings.FirstOrDefault(x => x.SourceType == typeof(IInterfaceA));
            Assert.That(typeMapping, Is.Not.Null);
            Assert.That(typeMapping.MappedType, Is.EqualTo(typeof(ImplA)));
            Assert.That(typeMapping.CreationInstruction, Is.EqualTo(CreationInstruction.Always));

            typeMapping = mappings.FirstOrDefault(x => x.SourceType == typeof(IInterfaceB));
            Assert.That(typeMapping, Is.Not.Null);
            Assert.That(typeMapping.MappedType, Is.EqualTo(typeof(ImplB)));
            Assert.That(typeMapping.CreationInstruction, Is.EqualTo(CreationInstruction.Scope));

            typeMapping = mappings.FirstOrDefault(x => x.SourceType == typeof(ImplC));
            Assert.That(typeMapping, Is.Not.Null);
            Assert.That(typeMapping.MappedType, Is.EqualTo(typeof(ImplC)));
            Assert.That(typeMapping.CreationInstruction, Is.EqualTo(CreationInstruction.Singleton));
        }

        [TestCase(CreationInstruction.Always, false)]
        [TestCase(CreationInstruction.Always, true)]
        [TestCase(CreationInstruction.Scope, false)]
        [TestCase(CreationInstruction.Scope, true)]
        [TestCase(CreationInstruction.Singleton, false)]
        [TestCase(CreationInstruction.Singleton, true)]
        public void Build_OnlySingleCreationType_Success(CreationInstruction creationInstruction, bool createFile)
        {
            #region Xml Creation

            string[] mappingsXml = new string[]
                    {
                        CreateMappingXml(typeof(IInterfaceA), typeof(ImplA), null),
                        CreateMappingXml(typeof(IInterfaceB), typeof(ImplB), null),
                        CreateMappingXml(typeof(IInterfaceC), typeof(ImplC), null),
                    };
            string xml;

            switch (creationInstruction)
            {
                case CreationInstruction.Always:
                    xml = CreateXml(string.Empty, CreateAlwaysMappings(mappingsXml), string.Empty, string.Empty);
                    break;
                case CreationInstruction.Scope:
                    xml = CreateXml(string.Empty, string.Empty, CreateScopeMappings(mappingsXml), string.Empty);
                    break;
                case CreationInstruction.Singleton:
                    xml = CreateXml(string.Empty, string.Empty, string.Empty, CreateSingletonMappings(mappingsXml));
                    break;
                default:
                    throw new NotSupportedException($"Unknown creation instruction {creationInstruction}!");
            }

            if (createFile)
            {
                File.WriteAllText(mTestXmlFileName, xml);
                mBuilder.ParseFile(mTestXmlFileName);
            }
            else
            {
                // passing string to a stream and parse the stream
                ParseString(xml);
            }

            #endregion

            IInjeCtor injeCtor = mBuilder.Build();

            Assert.That(injeCtor, Is.Not.Null);

            Assert.That(injeCtor.Mapper, Is.InstanceOf<SimpleTypeMapper>());
            Assert.That(injeCtor.TypeInformationProvider, Is.InstanceOf<TypeInformationBuilder>());
            Assert.That(injeCtor.TypeInformationBuilder, Is.InstanceOf<TypeInformationBuilder>());

            IReadOnlyList<ITypeMapping>? mappings = injeCtor.Mapper.GetTypeMappings();

            Assert.That(mappings, Is.Not.Null);
            Assert.That(mappings.Count, Is.EqualTo(6));

            ITypeMapping? typeMapping = mappings.FirstOrDefault(x => x.SourceType == typeof(IInterfaceA));
            Assert.That(typeMapping, Is.Not.Null);
            Assert.That(typeMapping.MappedType, Is.EqualTo(typeof(ImplA)));
            Assert.That(typeMapping.CreationInstruction, Is.EqualTo(creationInstruction));

            typeMapping = mappings.FirstOrDefault(x => x.SourceType == typeof(IInterfaceB));
            Assert.That(typeMapping, Is.Not.Null);
            Assert.That(typeMapping.MappedType, Is.EqualTo(typeof(ImplB)));
            Assert.That(typeMapping.CreationInstruction, Is.EqualTo(creationInstruction));

            typeMapping = mappings.FirstOrDefault(x => x.SourceType == typeof(IInterfaceC));
            Assert.That(typeMapping, Is.Not.Null);
            Assert.That(typeMapping.MappedType, Is.EqualTo(typeof(ImplC)));
            Assert.That(typeMapping.CreationInstruction, Is.EqualTo(creationInstruction));

            Assert.That(TestCreator.CreationCount, Is.EqualTo(0));
        }

        [TestCase(CreationInstruction.Always, false)]
        [TestCase(CreationInstruction.Always, true)]
        [TestCase(CreationInstruction.Scope, false)]
        [TestCase(CreationInstruction.Scope, true)]
        [TestCase(CreationInstruction.Singleton, false)]
        [TestCase(CreationInstruction.Singleton, true)]
        public void Build_OnlySingleCreationType_DirectType_Success(CreationInstruction creationInstruction, bool createFile)
        {
            #region Xml Creation

            string[] mappingsXml = new string[]
                    {
                        CreateMappingXml(typeof(ImplA), null, true),
                        CreateMappingXml(typeof(ImplB), null, true),
                        CreateMappingXml(typeof(ImplC), null, true),
                    };
            string xml;

            switch (creationInstruction)
            {
                case CreationInstruction.Always:
                    xml = CreateXml(string.Empty, CreateAlwaysMappings(mappingsXml), string.Empty, string.Empty);
                    break;
                case CreationInstruction.Scope:
                    xml = CreateXml(string.Empty, string.Empty, CreateScopeMappings(mappingsXml), string.Empty);
                    break;
                case CreationInstruction.Singleton:
                    xml = CreateXml(string.Empty, string.Empty, string.Empty, CreateSingletonMappings(mappingsXml));
                    break;
                default:
                    throw new NotSupportedException($"Unknown creation instruction {creationInstruction}!");
            }

            if (createFile)
            {
                File.WriteAllText(mTestXmlFileName, xml);
                mBuilder.ParseFile(mTestXmlFileName);
            }
            else
            {
                // passing string to a stream and parse the stream
                ParseString(xml);
            }

            #endregion

            IInjeCtor injeCtor = mBuilder.Build();

            Assert.That(injeCtor, Is.Not.Null);

            Assert.That(injeCtor.Mapper, Is.InstanceOf<SimpleTypeMapper>());
            Assert.That(injeCtor.TypeInformationProvider, Is.InstanceOf<TypeInformationBuilder>());
            Assert.That(injeCtor.TypeInformationBuilder, Is.InstanceOf<TypeInformationBuilder>());

            IReadOnlyList<ITypeMapping>? mappings = injeCtor.Mapper.GetTypeMappings();

            Assert.That(mappings, Is.Not.Null);
            Assert.That(mappings.Count, Is.EqualTo(6));

            ITypeMapping? typeMapping = mappings.FirstOrDefault(x => x.SourceType == typeof(ImplA));
            Assert.That(typeMapping, Is.Not.Null);
            Assert.That(typeMapping.MappedType, Is.EqualTo(typeof(ImplA)));
            Assert.That(typeMapping.CreationInstruction, Is.EqualTo(creationInstruction));

            typeMapping = mappings.FirstOrDefault(x => x.SourceType == typeof(ImplB));
            Assert.That(typeMapping, Is.Not.Null);
            Assert.That(typeMapping.MappedType, Is.EqualTo(typeof(ImplB)));
            Assert.That(typeMapping.CreationInstruction, Is.EqualTo(creationInstruction));

            typeMapping = mappings.FirstOrDefault(x => x.SourceType == typeof(ImplC));
            Assert.That(typeMapping, Is.Not.Null);
            Assert.That(typeMapping.MappedType, Is.EqualTo(typeof(ImplC)));
            Assert.That(typeMapping.CreationInstruction, Is.EqualTo(creationInstruction));

            Assert.That(TestCreator.CreationCount, Is.EqualTo(0));
        }

        [TestCase(CreationInstruction.Always, false)]
        [TestCase(CreationInstruction.Always, true)]
        [TestCase(CreationInstruction.Scope, false)]
        [TestCase(CreationInstruction.Scope, true)]
        [TestCase(CreationInstruction.Singleton, false)]
        [TestCase(CreationInstruction.Singleton, true)]
        public void Build_OnlySingleCreationType_DynamicTypeMapper_Success(CreationInstruction creationInstruction, bool createFile)
        {
            #region Xml Creation

            string[] mappingsXml = new string[]
                    {
                        CreateMappingXml(typeof(IInterfaceA), null, null),
                        CreateMappingXml(typeof(IInterfaceB), null, null),
                        CreateMappingXml(typeof(IInterfaceC), null, null),
                    };
            string xml;

            switch (creationInstruction)
            {
                case CreationInstruction.Always:
                    xml = CreateXml(CreateInjeCtorXml(null, null, null, true), CreateAlwaysMappings(mappingsXml), string.Empty, string.Empty);
                    break;
                case CreationInstruction.Scope:
                    xml = CreateXml(CreateInjeCtorXml(null, null, null, true), string.Empty, CreateScopeMappings(mappingsXml), string.Empty);
                    break;
                case CreationInstruction.Singleton:
                    xml = CreateXml(CreateInjeCtorXml(null, null, null, true), string.Empty, string.Empty, CreateSingletonMappings(mappingsXml));
                    break;
                default:
                    throw new NotSupportedException($"Unknown creation instruction {creationInstruction}!");
            }

            if (createFile)
            {
                File.WriteAllText(mTestXmlFileName, xml);
                mBuilder.ParseFile(mTestXmlFileName);
            }
            else
            {
                // passing string to a stream and parse the stream
                ParseString(xml);
            }

            #endregion

            IInjeCtor injeCtor = mBuilder.Build();

            Assert.That(injeCtor, Is.Not.Null);

            Assert.That(injeCtor.Mapper, Is.InstanceOf<DynamicTypeMapper>());
            Assert.That(injeCtor.TypeInformationProvider, Is.InstanceOf<TypeInformationBuilder>());
            Assert.That(injeCtor.TypeInformationBuilder, Is.InstanceOf<TypeInformationBuilder>());

            IReadOnlyList<ITypeMapping>? mappings = injeCtor.Mapper.GetTypeMappings();

            Assert.That(mappings, Is.Not.Null);
            Assert.That(mappings.Count, Is.EqualTo(6));

            ITypeMapping? typeMapping = mappings.FirstOrDefault(x => x.SourceType == typeof(IInterfaceA));
            Assert.That(typeMapping, Is.Not.Null);
            Assert.That(typeMapping.MappedType, Is.EqualTo(typeof(ImplA)));
            Assert.That(typeMapping.CreationInstruction, Is.EqualTo(creationInstruction));

            typeMapping = mappings.FirstOrDefault(x => x.SourceType == typeof(IInterfaceB));
            Assert.That(typeMapping, Is.Not.Null);
            Assert.That(typeMapping.MappedType, Is.EqualTo(typeof(ImplB)));
            Assert.That(typeMapping.CreationInstruction, Is.EqualTo(creationInstruction));

            typeMapping = mappings.FirstOrDefault(x => x.SourceType == typeof(IInterfaceC));
            Assert.That(typeMapping, Is.Not.Null);
            Assert.That(typeMapping.MappedType, Is.EqualTo(typeof(ImplC)));
            Assert.That(typeMapping.CreationInstruction, Is.EqualTo(creationInstruction));

            Assert.That(TestCreator.CreationCount, Is.EqualTo(0));
        }

        #endregion

        #region Private Methods

        private void ParseString(string xml)
        {
            using (MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(xml)))
            {
                mBuilder.ParseStream(ms);
            }
        }

        private string CreateXml(string injeCtorXml, string alwaysMappingXml, string scopeMappingXml, string singletonMappingXml)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine($"<Config xmlns=\"http://InjeCtor/Configuration/Xml\">");

            if (!string.IsNullOrEmpty(injeCtorXml))
                builder.Append($"   {injeCtorXml}");

            builder.AppendLine($"   <Mappings>");

            if (!string.IsNullOrEmpty(alwaysMappingXml))
                builder.Append($"       {alwaysMappingXml}");

            if (!string.IsNullOrEmpty(scopeMappingXml))
                builder.Append($"       {scopeMappingXml}");

            if (!string.IsNullOrEmpty(singletonMappingXml))
                builder.Append($"       {singletonMappingXml}");

            builder.AppendLine($"   </Mappings>");

            builder.AppendLine($"</Config>");
            return builder.ToString();
        }

        private string CreateInjeCtorXml(Type? typeMapperType, Type? informationBuilderType, Type? creatorType, bool? useDynamicTypeMapper)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("<InjeCtor");

            if (useDynamicTypeMapper.HasValue)
                builder.Append($" UseDynamicTypeMapper=\"{useDynamicTypeMapper.Value.ToString().ToLower()}\"");

            builder.AppendLine(">");

            if (typeMapperType != null)
                builder.AppendLine($"   <TypeMapper>{typeMapperType.FullName}, {typeMapperType.Assembly.GetName().Name}</TypeMapper>");

            if (informationBuilderType != null)
                builder.AppendLine($"   <TypeInformationBuilder>{informationBuilderType.FullName}, {informationBuilderType.Assembly.GetName().Name}</TypeInformationBuilder>");

            if (creatorType != null)
                builder.AppendLine($"   <Creator>{creatorType.FullName}, {creatorType.Assembly.GetName().Name}</Creator>");

            builder.AppendLine("</InjeCtor>");

            return builder.ToString();
        }

        private string CreateMappingXml(Type sourceType, Type? mappingType, bool? directType)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append($"<Mapping SourceType=\"{sourceType.FullName}, {sourceType.Assembly.GetName().Name}\"");

            if (mappingType != null)
                builder.Append($" MappingType=\"{mappingType.FullName}, {sourceType.Assembly.GetName().Name}\"");

            if (directType.HasValue)
                builder.Append($" DirectType=\"{directType.Value.ToString().ToLower()}\"");

            builder.AppendLine("/>");

            return builder.ToString();
        }

        private string CreateAlwaysMappings(params string[] mappings)
        {
            return CreateMappingList("Always", mappings);
        }

        private string CreateScopeMappings(params string[] mappings)
        {
            return CreateMappingList("ScopeSingleton", mappings);
        }

        private string CreateSingletonMappings(params string[] mappings)
        {
            return CreateMappingList("Singleton", mappings);
        }

        private string CreateMappingList(string creationInstructionElementName, params string[] mappings)
        {
            if (mappings is null || !mappings.Any())
                return string.Empty;

            StringBuilder builder = new StringBuilder();
            builder.AppendLine($"<{creationInstructionElementName}>");

            foreach (string mapping in mappings)
                builder.AppendLine($"   {mapping}");

            builder.AppendLine($"</{creationInstructionElementName}>");
            return builder.ToString();
        }

        #endregion
    }
}