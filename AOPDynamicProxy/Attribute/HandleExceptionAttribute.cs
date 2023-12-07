using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AOPDynamicProxy
{
    /// <summary>
    /// 捕获异常的策略
    /// 若strategy含忽略异常,则必须添加IAfterInterceptor拦截器并在其中设置ReturnValue，否则将抛出'InvalidOperationException'
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
    public class HandleExceptionAttribute : Attribute
    {
        /// <summary>
        /// 需捕获的异常类型
        /// </summary>
        public Type ExType { get; private set; }

        /// <summary>
        /// 异常处理策略
        /// </summary>
        public EnumExceptionStrategy ExStrategy { get; private set; }

        /// <summary>
        /// 是否记录日志(默认为True)
        /// </summary>
        public bool NeedLog { get; set; } = true;

        /// <summary>
        /// 返回值 若目标方法有返回值,且在执行时被捕获到指定的[ExceptionType]异常,可以此设置方法返回值
        /// </summary>
        public object ReturnValue { get; set; }

        /// <summary>
        /// 额外msg 若目标方法在执行时被捕获到指定的[ExceptionType]异常,可以此提供额外的信息
        /// </summary>
        public string ExtraMsg { get; set; }

        /// <summary>
        /// 在何版本下 处理此异常
        /// </summary>
        public EnumFunctionalVersion HandleVersion { get; set; }

        public HandleExceptionAttribute(Type exType, EnumExceptionStrategy exStrategy)
        {
            ExType = exType;
            ExStrategy = exStrategy;
        }
    }
}
