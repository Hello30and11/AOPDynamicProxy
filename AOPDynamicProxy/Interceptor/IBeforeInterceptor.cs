using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AOPDynamicProxy
{
    public interface IBeforeInterceptor : ICustomInterceptor
    {
        void Intercept(IBeforeInvocation beforeInvocation);
    }
}
