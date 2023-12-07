using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AOPDynamicProxy
{
    public enum EnumExceptionStrategy
    {
        /// <summary>
        /// 忽略异常
        /// </summary>
        ignore = 0,

        /// <summary>
        /// 再次抛出异常
        /// </summary>
        throwAgain = 1
    }
}
