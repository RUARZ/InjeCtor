using InjeCtor.Configuration.Xml;
using InjeCtor.Core;
using InjeCtor.Core.Samples.Interfaces;
using InjeCtor.Core.Scope;

namespace InjeCtor.Configuration.Samples
{
    public static class Program
    {
        static void Main(string[] args)
        {
            using IInjeCtor injeCtor = CreateFromXml();

            IUserInteraction userInteraction = injeCtor.Get<IUserInteraction>();

            userInteraction.Inform("Hello World");

            string response = userInteraction.Ask<string>("Shut down application (y/n)?");

            if (response == "y")
                return;

            int value = userInteraction.GetInput<int>("Enter a number:");
            userInteraction.Inform($"Input number: {value}");

            IPlayer player = injeCtor.Get<IPlayer>();

            player.StartRunning();
            player.Hide();
            player.Attack();
            player.DecreaseHP(80);
            player.IncreaseHP(55);
            player.DecreaseHP(111);
            player.Hide();

            GetParametersForContext(injeCtor);
            SetResultForContext(injeCtor);

            IRequestContext context = injeCtor.Get<IRequestContext>();
            userInteraction.Inform($"Parameters: {string.Join(", ", context.Parameters)}");
            userInteraction.Inform($"Results: {string.Join(", ", context.ResultValues)}");

            using IScope scope = injeCtor.CreateScope();

            IRequestContext context2 = scope.Get<IRequestContext>();

            userInteraction.Inform($"Context and Context2 reference equals: {ReferenceEquals(context, context2)}");

            userInteraction.GetInput<string>("Finished Program, press eny key to quit");
        }

        private static void GetParametersForContext(IInjeCtor injector)
        {
            IRequestContext context = injector.Get<IRequestContext>();

            context.Parameters = new object[] { "one", 2, 3, "four", 5.67 };
        }

        private static void SetResultForContext(IInjeCtor injector)
        {
            IRequestContext context = injector.Get<IRequestContext>();

            context.ResultValues = new object[] { "SUCCESS" };
        }

        private static IInjeCtor CreateFromXml()
        {
            XmlInjeCtorBuilder builder = new XmlInjeCtorBuilder();

            bool fromFile = true;
            
            // you can parse a xml with the configuration for the injector mappings ...

            if (fromFile)
            {
                // .. from a file to read.
                string? pathToXml = Path.GetDirectoryName(typeof(Program).Assembly.Location);

                pathToXml = Path.Combine(pathToXml, "Xml", "ConfigurationSample.xml");

                builder.ParseFile(pathToXml);
            }
            else
            {
                // .. or read from an stream (e.g. from an embedded stream or from other streams)
                string xmlManifestResourceName = "InjeCtor.Configuration.Samples.Xml.ConfigurationSample.xml";

                Stream streamWithXml = typeof(Program).Assembly.GetManifestResourceStream(xmlManifestResourceName);

                builder.ParseStream(streamWithXml);
            }

            return builder.Build();
        }
    }
}