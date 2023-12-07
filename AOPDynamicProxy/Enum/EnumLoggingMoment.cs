using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AOPDynamicProxy
{
    /// <summary>
    /// 记录日志的时间点
    /// </summary>
    public enum EnumLoggingMoment
    {
        /// <summary>
        /// 目标方法调用前
        /// </summary>
        beforeProceed = 0,

        /// <summary>
        /// 目标方法调用后
        /// </summary>
        afterProceed = 1
    }
}
