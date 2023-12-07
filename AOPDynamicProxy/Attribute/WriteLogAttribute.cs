using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AOPDynamicProxy
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
    public class WriteLogAttribute : Attribute
    {
        /// <summary>
        /// 记录日志的时间
        /// 目标方法执行前||目标方法执行后
        /// </summary>
        public EnumLoggingMoment LogMoment { get; private set; }

        /// <summary>
        /// 日志内容
        /// </summary>
        public string Content { get; private set; }

        /// <summary>
        /// 在何版本下记录此日志
        /// </summary>
        public EnumFunctionalVersion LogVersion { get; set; }

        public WriteLogAttribute(EnumLoggingMoment logMoment, string content)
        {
            this.LogMoment = logMoment;
            this.Content = content;
        }
    }
}
