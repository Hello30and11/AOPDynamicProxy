using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AOPDynamicProxy
{
    public interface IAfterInterceptor : ICustomInterceptor
    {
        void Intercept(IAfterInvocation afterInvocation);
    }
}
