using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AOPDynamicProxy
{
    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class | AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
    public class BindInterceptorAttribute : Attribute
    {
        public Type InterceptorType { get; private set; }

        /// <summary>
        /// 拦截器调用顺序
        /// </summary>
        public byte SerialNo { get; set; }

        /// <summary>
        /// 绑定拦截器的构造器实参
        /// 只可为[常量]、[typeof]或[数组创建]的表达式【其中，数组的项也只可为[常量]OR[typeof]表达式】
        /// </summary>
        public object[] InterceptorConstructArgs { get; set; }

        public BindInterceptorAttribute(Type interceptorType)
        {
            InterceptorType = interceptorType;
            SerialNo = byte.MaxValue; //默认为byte.MaxValue
        }

        public BindInterceptorAttribute(Type interceptorType, params object[] interceptorConstructArgs) : this(interceptorType)
        {
            InterceptorConstructArgs = interceptorConstructArgs;
        }
    }
}
