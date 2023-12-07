using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AOPDynamicProxy
{
    /// <summary>
    /// 作为自定义特性的基类，使用此AOP框架的程序可定义派生自此类的特性，然后在[IAfterInterceptor][IAfterInterceptor]中使用
    /// </summary>
    public class CustomAttribute : Attribute
    {
    }
}
