using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AOPDynamicProxy
{
    public interface IAfterInvocation : ICustomInvocation
    {
        object ReturnValue { get; set; }
    }
}
