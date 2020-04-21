using System;
using System.Collections.Generic;

namespace EventFrameworkTestBed.Creator
{
    public class ObjectFactoryOptions
    {
        public Func<object> Create { get; set; } = () => null;
        public Func<IEnumerable<object>> CreateMulti { get; set; } = () => null;
    }
}
