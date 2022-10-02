using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using System.Threading.Tasks;

namespace InjeCtor.Core.Test.RuntimeLoading
{
    internal class TestAssemblyLoadContext : AssemblyLoadContext
    {
        public TestAssemblyLoadContext() : base(true)
        {

        }

        protected override Assembly? Load(AssemblyName assemblyName)
        {
            return null;
        }
    }
}
