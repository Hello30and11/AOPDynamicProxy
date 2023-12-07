using Castle.DynamicProxy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AOPDynamicProxy
{
    public class BeforeInvocation : IBeforeInvocation
    {
        private IInvocation m_invocation;
        public object[] Arguments => m_invocation.Arguments;
        public Type[] GenericArguments => m_invocation.GenericArguments;

        private bool m_canInvokeTargetMethod;
        /// <summary>
        /// 指示是否应该调用被代理的目标方法
        /// </summary>
        public bool CanInvokeTargetMethod { get => m_canInvokeTargetMethod; set => m_canInvokeTargetMethod = value; }

        public List<CustomAttribute> CustomAttributes { get; private set; }

        public object Logger { get; private set; }

        /// <summary>
        /// 构造器
        /// </summary>
        /// <param name="invocation"></param>
        /// <param name="customAttributes">自定义Attribute</param>
        public BeforeInvocation(IInvocation invocation, List<CustomAttribute> customAttributes, object logger)
        {
            m_invocation = invocation;
            CustomAttributes = customAttributes;
            Logger = logger;
            m_canInvokeTargetMethod = true; //默认可调用被代理的目标方法
        }

        public object GetArgumentValue(int index)
        {
            return m_invocation.GetArgumentValue(index);
        }

        public void SetArgumentValue(int index, object value)
        {
            m_invocation.SetArgumentValue(index, value);
        }

        public void SetReturnValue(object returnValue)
        {
            m_invocation.ReturnValue = returnValue;
        }
    }
}
