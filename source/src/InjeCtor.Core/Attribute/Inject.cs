using System;

namespace InjeCtor.Core.Attribute
{
    /// <summary>
    /// Attribute to mark properties, methods, ... to inject types if found within mappings
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Method)]
    public class Inject : System.Attribute
    {
    }
}
