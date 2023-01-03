using InjeCtor.Core.TypeInformation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace InjeCtor.Configuration.Test.TestImplementations
{
    public class TestTypeInformationBuilder : ITypeInformationBuilder, ITypeInformationProvider
    {
        public void Add<T>()
        {
            throw new NotImplementedException();
        }

        public void Add(Type type)
        {
            throw new NotImplementedException();
        }

        public bool AddPropertyInjection<T, TProperty>(Expression<Func<T, TProperty>> expression)
        {
            throw new NotImplementedException();
        }

        public ITypeInformation? Get<T>()
        {
            throw new NotImplementedException();
        }

        public ITypeInformation? Get(Type type)
        {
            throw new NotImplementedException();
        }
    }
}
