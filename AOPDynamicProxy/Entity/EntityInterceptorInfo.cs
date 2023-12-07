using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AOPDynamicProxy
{
    /// <summary>
    /// 拦截器 实体Info
    /// </summary>
    internal class EntityInterceptorInfo
    {
        public EntityInterceptorInfo(byte serialNo, ICustomInterceptor interceptor)
        {
            this.SerialNo = serialNo;
            this.Interceptor = interceptor;
        }

        /// <summary>
        /// 拦截器 调用序号
        /// </summary>
        public byte SerialNo { get; private set; }

        /// <summary>
        /// 拦截器对象
        /// </summary>
        public ICustomInterceptor Interceptor { get; private set; }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            var interceptorWithNoObj = obj as EntityInterceptorInfo;
            if (interceptorWithNoObj == null)
            {
                return false;
            }
            if (interceptorWithNoObj.SerialNo != this.SerialNo)
            {
                return false;
            }
            if (interceptorWithNoObj.Interceptor.GetType() != this.Interceptor.GetType())
            {
                return false;
            }
            return true;
        }

        public override int GetHashCode()
        {
            return this.SerialNo ^ this.Interceptor.GetType().GetHashCode();
        }
    }
}
