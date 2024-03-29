﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InjeCtor.Core.Samples.Interfaces
{
    public interface IRequestContext
    {
        IEnumerable<object> Parameters { get; set; }

        IEnumerable<object> ResultValues { get; set; }
    }
}
