using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AOPDynamicProxy
{
    /// <summary>
    /// 起作用的程序版本
    /// debug(调试版本) OR release(发布版本) OR all(所有版本)
    /// </summary>
    public enum EnumFunctionalVersion
    {
        /// <summary>
        /// 所有版本
        /// </summary>
        ALLVERSION = 0,

        /// <summary>
        /// 调试版本
        /// </summary>
        DEBUG = 1,

        /// <summary>
        /// 发布版本
        /// </summary>
        RELEASE = 2
    }
}
