using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AOPDynamicProxy
{
    public interface ICustomInvocation
    {
        object[] Arguments { get; }

        Type[] GenericArguments { get; }

        List<CustomAttribute> CustomAttributes { get; }

        object Logger { get; }

        object GetArgumentValue(int index);
    }
}
