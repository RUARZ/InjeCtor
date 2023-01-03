using System;

namespace InjeCtor.Configuration.Xml
{

    [Serializable]
    public class InvalidXmlConfigurationException : Exception
    {
        public InvalidXmlConfigurationException() { }
        public InvalidXmlConfigurationException(string message) : base(message) { }
        public InvalidXmlConfigurationException(string message, Exception inner) : base(message, inner) { }
        protected InvalidXmlConfigurationException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
