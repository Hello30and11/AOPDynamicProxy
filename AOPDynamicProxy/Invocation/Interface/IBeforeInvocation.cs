using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AOPDynamicProxy
{
    public interface IBeforeInvocation : ICustomInvocation
    {
        bool CanInvokeTargetMethod { get; set; }

        void SetReturnValue(object returnValue);

        void SetArgumentValue(int index, object value);
    }
}
