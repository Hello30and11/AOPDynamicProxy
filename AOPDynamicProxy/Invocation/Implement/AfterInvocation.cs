using Castle.DynamicProxy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AOPDynamicProxy
{
    public class AfterInvocation : IAfterInvocation
    {
        private IInvocation m_invocation;
        public object[] Arguments => m_invocation.Arguments;

        public Type[] GenericArguments => m_invocation.GenericArguments;

        public object ReturnValue { get => m_invocation.ReturnValue; set => m_invocation.ReturnValue = value; }

        public List<CustomAttribute> CustomAttributes { get; private set; }

        public object Logger { get; private set; }

        /// <summary>
        /// 构造器
        /// </summary>
        /// <param name="invocation"></param>
        /// <param name="customAttributes">自定义Attribute</param>
        public AfterInvocation(IInvocation invocation, List<CustomAttribute> customAttributes, object logger)
        {
            m_invocation = invocation;
            CustomAttributes = customAttributes;
            Logger = logger;
        }

        public object GetArgumentValue(int index)
        {
            return m_invocation.GetArgumentValue(index);
        }
    }
}
