using InjeCtor.Core.Samples.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InjeCtor.Core.Samples.Implementations
{
    internal class RequestContext : IRequestContext
    {
        public RequestContext()
        {
            Parameters = Enumerable.Empty<object>();
            ResultValues = Enumerable.Empty<object>();
        }

        public IEnumerable<object> Parameters { get; set; }

        public IEnumerable<object> ResultValues { get; set; }
    }
}
