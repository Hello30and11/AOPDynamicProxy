using Castle.Core.Logging;
using Castle.DynamicProxy;
using log4net;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;

namespace AOPDynamicProxy
{
    /// <summary>
    /// 基础拦截器，所有使用[DynamicProxyFactory]中方法创建的目标对象都将绑定此拦截器
    /// 以Attribute方式进行[记录日志]、[捕获异常]、[记录方法运行时间]
    /// </summary>
    internal sealed class CastleInterceptor : IInterceptor
    {
        /// <summary>
        /// 方法及之上绑定的拦截器List 字典
        /// </summary>
        private Dictionary<MethodInfo, List<EntityInterceptorInfo>> m_customInterceptorsOnMethods;
        private ILogger m_logger;
        private Stopwatch m_stopwatch;

        public CastleInterceptor(Dictionary<MethodInfo, List<EntityInterceptorInfo>> customInterceptors, ILogger logger)
        {
            this.m_customInterceptorsOnMethods = customInterceptors;
            this.m_logger = logger;
        }

        /// <summary>
        /// 拦截器方法
        /// </summary>
        /// <param name="invocation"></param>
        public void Intercept(IInvocation invocation)
        {
            #region 
            //invocation.SetArgumentValue(0, 6); //设置方法参数的值
            //var arguments = invocation.Arguments; //方法参数
            //var genericArguments = invocation.GenericArguments; //泛型方法的泛型参数，非泛型方法则为null
            //var invocationTarget = invocation.InvocationTarget; //调用目标，大多数情况下为代理的目标对象
            //var proxy = invocation.Proxy; //生成的动态代理对象
            //var returnValue = invocation.ReturnValue; //方法的返回值
            //var targetType = invocation.TargetType; //目标对象的类型
            //var captureProceedInfo = invocation.CaptureProceedInfo();
            //var specifiedArgumentValue = invocation.GetArgumentValue(0); //获取指定index的参数值
            #endregion

            //目标方法绑定的全部特性
            var attributesOnTargetMethod = GetAttributesBindedOnTargetMethod(invocation);

            //目标方法绑定的[CustomAttribute]特性 OR 派生自[CustomAttribute]的特性
            var customAttrsByUser = GetDesignatedTypeCompatibleAttrs<CustomAttribute>(attributesOnTargetMethod);

            object logger = GetSettedLogger(); //获取用户设置的logger【目前只支持log4net.ILog对象。可能为null】

            bool shouldInvokeTargetMethod = InvokeBeforeInterceptMethods(invocation, customAttrsByUser, logger);
            if (shouldInvokeTargetMethod)
            {
                if (invocation.InvocationTarget != null)
                {
                    InvokeTargetMethod(invocation, attributesOnTargetMethod); //调用被代理的目标方法
                }
                else
                {
                    //部分方式创建代理对象时，InvocationTarget == null，此时可能导致2个问题：
                    //①若调用invocation.Proceed()，程序将抛出异常；
                    //②若目标方法有返回值，则需调用者自行设置ReturnValue(或通过绑定CustomInterceptor处理；或通过ExceptionHandleAttribute处理)
                    //  若目标方法有返回值的情况下未对ReturnValue做处理，会抛出异常
                }
                InvokeAfterInterceptMethods(invocation, customAttrsByUser, logger);
            }
        }



        #region 私有方法
        /// <summary>
        /// 调用Before拦截器方法
        /// </summary>
        /// <param name="invocation"></param>
        /// <returns>是否调用目标方法及After拦截器方法 true:调用 false:不调用</returns>
        private bool InvokeBeforeInterceptMethods(IInvocation invocation, List<CustomAttribute> customAttrsByUser, object logger)
        {
            bool shouldInvokeTargetMethod = true; //标识是否调用目标方法
            var beforeInterceptors = GetDesignatedInterceptors<IBeforeInterceptor>(invocation);
            if (beforeInterceptors.Count > 0)
            {
                IBeforeInvocation beforeInvocation = new BeforeInvocation(invocation, customAttrsByUser, logger);
                for (int i = 0; i < beforeInterceptors.Count; i++)
                {
                    var beforeInterceptor = beforeInterceptors[i];
                    beforeInterceptor.Intercept(beforeInvocation); //依次调用每一个before拦截器方法

                    if (!beforeInvocation.CanInvokeTargetMethod)
                        shouldInvokeTargetMethod = false;
                }
            }
            return shouldInvokeTargetMethod;
        }

        /// <summary>
        /// 调用After拦截器方法
        /// </summary>
        /// <param name="invocation"></param>
        private void InvokeAfterInterceptMethods(IInvocation invocation, List<CustomAttribute> customAttrsByUser, object logger)
        {
            var afterInterceptors = GetDesignatedInterceptors<IAfterInterceptor>(invocation);
            if (afterInterceptors.Count > 0)
            {
                IAfterInvocation afterInvocation = new AfterInvocation(invocation, customAttrsByUser, logger);
                for (int i = 0; i < afterInterceptors.Count; i++)
                {
                    var afterInterceptor = afterInterceptors[i];
                    afterInterceptor.Intercept(afterInvocation); //依次调用每一个after拦截器方法
                }
            }
        }

        /// <summary>
        /// 调用目标方法
        /// </summary>
        /// <param name="invocation"></param>
        private void InvokeTargetMethod(IInvocation invocation, List<Attribute> attributesOnTargetMethod)
        {
            //目标方法绑定的[HandleExceptionAttribute]特性
            var handleExAttrs = GetDesignatedTypeAttrs<HandleExceptionAttribute>(attributesOnTargetMethod);

            //目标方法绑定的[WriteLogAttribute]特性
            var writeLogAttrs = GetDesignatedTypeAttrs<WriteLogAttribute>(attributesOnTargetMethod);

            //目标方法绑定的[NoteElapsedTimeAttribute]特性
            var NoteElapsedTimeAttr = GetDesignatedTypeAttrs<NoteElapsedTimeAttribute>(attributesOnTargetMethod).FirstOrDefault();

            //记录logTime=beforeProceed的日志Content
            WriteLogsByWriteLogAttribute(EnumLoggingMoment.beforeProceed, writeLogAttrs);

            if (NoteElapsedTimeAttr != null)
            {
                m_stopwatch = m_stopwatch == null ? new Stopwatch() : m_stopwatch;
                m_stopwatch.Restart(); //开始计时
                InvokeProxiedMethodOnCatchEx(invocation, handleExAttrs);
                m_stopwatch.Stop(); //结束计时
                NoteElapsedTimeOnMethod(invocation, NoteElapsedTimeAttr); //记录耗时
            }
            else
            {
                InvokeProxiedMethodOnCatchEx(invocation, handleExAttrs);
            }

            //记录logTime=afterProceed的日志
            WriteLogsByWriteLogAttribute(EnumLoggingMoment.afterProceed, writeLogAttrs);
        }

        /// <summary>
        /// 调用目标方法 并捕获指定异常
        /// </summary>
        /// <param name="invocation"></param>
        /// <param name="handleExAttrs"></param>
        private void InvokeProxiedMethodOnCatchEx(IInvocation invocation, List<HandleExceptionAttribute> handleExAttrs)
        {
            try
            {
                invocation.Proceed(); //调用目标方法
            }
            catch (Exception ex)
            {
                var catchedDesignatedExs = handleExAttrs.FindAll(item => item.ExType == ex.GetType());

                if (catchedDesignatedExs.Count <= 0)
                {
                    //指定要处理的异常(HandleExceptionAttribute方式指定)中不含抛出的异常，抛出异常并中止代码
                    throw;
                }
                //标识:在当前程序版本下对捕获到的指定异常是否需要按各个[HandleExceptionAttribute]指定的方式处理
                bool needHandleCatchedDesignatedEx;

                foreach (var catchedDesignatedEx in catchedDesignatedExs)
                {
                    needHandleCatchedDesignatedEx = false;

                    if (catchedDesignatedEx.HandleVersion == EnumFunctionalVersion.ALLVERSION)
                    {
                        needHandleCatchedDesignatedEx = true;
                        goto handleEx;
                    }
                    else
                    {
#if DEBUG
                        if (catchedDesignatedEx.HandleVersion == EnumFunctionalVersion.DEBUG)
                        {
                            needHandleCatchedDesignatedEx = true;
                            goto handleEx;
                        }
#else
                        if (catchedDesignatedEx.HandleVersion == EnumFunctionalVersion.RELEASE)
                        {
                            needHandleCatchedDesignatedEx = true;
                            goto handleEx;
                        }
#endif
                    }

                handleEx:
                    {
                        if (needHandleCatchedDesignatedEx) //捕获到的指定异常在当前版本下需处理，按[catchedDesignatedEx]指定的方式做处理
                        {
                            string extraMsg = string.IsNullOrWhiteSpace(catchedDesignatedEx.ExtraMsg)
                                         ? null
                                         : catchedDesignatedEx.ExtraMsg;

                            if (catchedDesignatedEx.NeedLog)
                            {
                                StringBuilder sb = new StringBuilder();
                                sb.Append($"方法{invocation.Method.Name}出现异常。");
                                if (extraMsg != null)
                                {
                                    sb.Append($"ExtraMsg：{extraMsg }；" +
                                              $"{Environment.NewLine}");
                                }
                                sb.Append($"Ex：{ex.ToString()}");

                                //记录日志，并继续运行
                                Logging(EnumLogLevel.Error, sb.ToString());
                            }
                            if (catchedDesignatedEx.ReturnValue != null)
                            {
                                //设置目标方法的ReturnValue【若目标方法返回void，直接设置invocation.ReturnValue也不会有问题,设置的ReturnValue将不起作用】
                                invocation.ReturnValue = catchedDesignatedEx.ReturnValue;
                            }
                            switch (catchedDesignatedEx.ExStrategy)
                            {
                                case EnumExceptionStrategy.ignore:
                                    {
                                        //忽略此异常，继续往下走
                                    }
                                    break;
                                case EnumExceptionStrategy.throwAgain:
                                default:
                                    {
                                        if (extraMsg != null)
                                        {
                                            if (ex.Data != null)
                                                ex.Data.Add("ExtraMsg", extraMsg);
                                        }
                                        //抛出异常，中止代码
                                        throw;
                                    }
                            }
                        }
                        else
                        {
                            //捕获到的指定异常在当前版本下无需处理，则抛出
                            throw;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 获取调用的方法 可能含目标方法&代理方法
        /// </summary>
        /// <param name="invocation"></param>
        /// <returns>返回方法List 可能为空List</returns>
        private List<MethodInfo> GetInvocationMethods(IInvocation invocation)
        {
            List<MethodInfo> methodInfos = new List<MethodInfo>();
            methodInfos.AddRange(new List<MethodInfo>() { invocation.Method,
                                                          invocation.MethodInvocationTarget,
                                                          invocation.GetConcreteMethod(),
                                                          invocation.GetConcreteMethodInvocationTarget() });
            methodInfos.RemoveAll(item => item == null);
            methodInfos = methodInfos?.Distinct()?.ToList() ?? new List<MethodInfo>();
            return methodInfos;
        }

        /// <summary>
        /// 获取被代理方法上绑定的Attribute
        /// </summary>
        /// <param name="invocation"></param>
        /// <returns></returns>
        private List<Attribute> GetAttributesBindedOnTargetMethod(IInvocation invocation)
        {
            List<Attribute> totalAttributes = new List<Attribute>();
            List<object[]> attributeArray = new List<object[]>();

            var methodInfos = GetInvocationMethods(invocation);
            foreach (var methodInfo in methodInfos)
            {
                if (methodInfo == null)
                    continue;
                var attributeObjs = methodInfo.GetCustomAttributes(true);
                foreach (var item in attributeObjs)
                {
                    Attribute attribute = item as Attribute;
                    if (!totalAttributes.Exists(att => att.Equals(attribute)))
                        totalAttributes.Add(attribute);
                }
            }
            return totalAttributes;
        }

        /// <summary>
        /// 获取目标方法绑定的全部[ICustomInterceptor]拦截器List
        /// </summary>
        /// <param name="invocation"></param>
        /// <returns>返回目标方法绑定的拦截器List 若无,则返回空的List</returns>
        private List<EntityInterceptorInfo> GetInterceptorsOnTargetMethod(IInvocation invocation)
        {
            List<EntityInterceptorInfo> customInterceptors = new List<EntityInterceptorInfo>();

            var methods = GetInvocationMethods(invocation);

            foreach (var methodInfo in methods)
            {
                this.m_customInterceptorsOnMethods.TryGetValue(methodInfo, out List<EntityInterceptorInfo> bindedInterceptors);
                if (bindedInterceptors != null && bindedInterceptors.Count > 0)
                {
                    customInterceptors.AddRange(bindedInterceptors);
                }
            }
            if (customInterceptors.Count <= 0)
            {
                //说明：GetInvocationMethods(invocation)返回的只有代理方法【部分创建代理类对象的方法,invocation中不会包含被代理方法的MethodInfo】
                //      OR 当前被代理方法未绑定ICustomInterceptor拦截器
                //?待验证?
            }
            return customInterceptors;
        }

        /// <summary>
        /// 获取目标方法绑定的指定类型拦截器List(如：before|after拦截器) 有序
        /// </summary>
        /// <typeparam name="IDesignatedInterceptor">指定类型的拦截器</typeparam>
        /// <param name="invocation"></param>
        /// <returns>返回目标方法绑定的指定类型拦截器 若无,则返回空的List</returns>
        private List<IDesignatedInterceptor> GetDesignatedInterceptors<IDesignatedInterceptor>(IInvocation invocation)
            where IDesignatedInterceptor : class, ICustomInterceptor
        {
            List<IDesignatedInterceptor> designatedInterceptors = new List<IDesignatedInterceptor>();

            if (this.m_customInterceptorsOnMethods == null || this.m_customInterceptorsOnMethods.Count <= 0)
                return designatedInterceptors;

            List<EntityInterceptorInfo> interceptorsOnTargetMethod = GetInterceptorsOnTargetMethod(invocation);
            interceptorsOnTargetMethod.OrderBy(item => item.SerialNo)
                                      .ToList()
                                      .ForEach(item =>
                                      {
                                          var methodInterceptor = item.Interceptor;
                                          if (methodInterceptor is IDesignatedInterceptor)
                                          {
                                              designatedInterceptors.Add(methodInterceptor as IDesignatedInterceptor);
                                          }
                                      });
            return designatedInterceptors;
        }

        /// <summary>
        /// 获取用户设置的logger【目前只支持log4net.ILog对象】
        /// </summary>
        /// <returns>logger对象 可能为null</returns>
        private object GetSettedLogger()
        {
            object logger = null;
            if (this.m_logger is Logger_log4net)
                logger = (m_logger as Logger_log4net)?.Log4netLogger;
            return logger;
        }

        /// <summary>
        /// 记录日志
        /// </summary>
        /// <param name="level"></param>
        /// <param name="content"></param>
        private void Logging(EnumLogLevel level, string content)
        {
            if (m_logger == null)
                return;

            switch (level)
            {
                case EnumLogLevel.Debug:
                    {
                        if (m_logger.IsDebugEnabled)
                            m_logger.Debug(content);
                    }
                    break;
                case EnumLogLevel.Info:
                    {
                        if (m_logger.IsInfoEnabled)
                            m_logger.Info(content);
                    }
                    break;
                case EnumLogLevel.Warn:
                    {
                        if (m_logger.IsWarnEnabled)
                            m_logger.Warn(content);
                    }
                    break;
                case EnumLogLevel.Error:
                    {
                        if (m_logger.IsErrorEnabled)
                            m_logger.Error(content);
                    }
                    break;
            }
        }

        /// <summary>
        /// 获取与指定[TAttribute]同类型的Attribute列表
        /// </summary>
        /// <typeparam name="TAttribute"></typeparam>
        /// <param name="attributes"></param>
        /// <returns></returns>
        private List<TAttribute> GetDesignatedTypeAttrs<TAttribute>(List<Attribute> attributes) where TAttribute : Attribute
        {
            List<TAttribute> designatedTypeAttrs = new List<TAttribute>();
            attributes.FindAll(item => item.GetType() == typeof(TAttribute))
                      .ForEach(item => designatedTypeAttrs.Add(item as TAttribute));
            return designatedTypeAttrs;
        }

        /// <summary>
        /// 获取与指定[TAttribute]兼容的Attribute列表
        /// </summary>
        /// <typeparam name="TAttribute"></typeparam>
        /// <param name="attributes"></param>
        /// <returns></returns>
        private List<TAttribute> GetDesignatedTypeCompatibleAttrs<TAttribute>(List<Attribute> attributes) where TAttribute : Attribute
        {
            List<TAttribute> designatedTypeAttrs = new List<TAttribute>();
            attributes.FindAll(item => item is TAttribute)
                      .ForEach(item => designatedTypeAttrs.Add(item as TAttribute));
            return designatedTypeAttrs;
        }

        /// <summary>
        /// 根据[WriteLogAttribute]信息记录日志
        /// </summary>
        /// <param name="logTime"></param>
        /// <param name="writeLogAttrs"></param>
        private void WriteLogsByWriteLogAttribute(EnumLoggingMoment logTime, List<WriteLogAttribute> writeLogAttrs)
        {
            writeLogAttrs.FindAll(item => item.LogMoment == logTime)
                         .ForEach(item =>
                         {
                             if (item.LogVersion == EnumFunctionalVersion.ALLVERSION)
                                 Logging(EnumLogLevel.Info, item.Content);
                             else
                             {
                                 LoggingOnDebug(item);
                                 LoggingOnRelease(item);
                             }
                         });
        }

        [Conditional("DEBUG")]
        private void LoggingOnDebug(WriteLogAttribute writeLogAttr)
        {
            if (writeLogAttr.LogVersion != EnumFunctionalVersion.DEBUG)
                return;
            Logging(EnumLogLevel.Info, writeLogAttr.Content);
        }

        [Conditional("RELEASE")]
        private void LoggingOnRelease(WriteLogAttribute writeLogAttr)
        {
            if (writeLogAttr.LogVersion != EnumFunctionalVersion.RELEASE)
                return;
            Logging(EnumLogLevel.Info, writeLogAttr.Content);
        }


        /// <summary>
        /// 将[Stopwatch]已获取到的执行时间做记录
        /// </summary>
        /// <param name="invocation"></param>
        /// <param name="noteElapsedTimeAttr"></param>
        /// <param name="stopwatch"></param>
        private void NoteElapsedTimeOnMethod(IInvocation invocation, NoteElapsedTimeAttribute noteElapsedTimeAttr)
        {
            if (noteElapsedTimeAttr.NoteVersion == EnumFunctionalVersion.ALLVERSION)
            {
                NoteElapsedTime(noteElapsedTimeAttr, invocation.Method.Name);
            }
            else
            {
                NoteElapsedTimeOnDebug(noteElapsedTimeAttr, invocation.Method.Name);
                NoteElapsedTimeOnRelease(noteElapsedTimeAttr, invocation.Method.Name);
            }

        }

        [Conditional("DEBUG")]
        private void NoteElapsedTimeOnDebug(NoteElapsedTimeAttribute noteElapsedTimeAttr, string methodName)
        {
            if (noteElapsedTimeAttr.NoteVersion != EnumFunctionalVersion.DEBUG)
                return;
            NoteElapsedTime(noteElapsedTimeAttr, methodName);
        }

        [Conditional("RELEASE")]
        private void NoteElapsedTimeOnRelease(NoteElapsedTimeAttribute noteElapsedTimeAttr, string methodName)
        {
            if (noteElapsedTimeAttr.NoteVersion != EnumFunctionalVersion.RELEASE)
                return;
            NoteElapsedTime(noteElapsedTimeAttr, methodName);
        }

        private void NoteElapsedTime(NoteElapsedTimeAttribute noteElapsedTimeAttr, string methodName)
        {
            if (noteElapsedTimeAttr.NoteMode == EnumElapsedTimeNoteMode.log)
            {
                Logging(EnumLogLevel.Info, $"方法{methodName}() 运行消耗：[{m_stopwatch.ElapsedMilliseconds}]毫秒");
            }
        }
        #endregion 私有方法
    }
}
