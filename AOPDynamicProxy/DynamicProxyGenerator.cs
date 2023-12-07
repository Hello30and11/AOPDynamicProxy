using Castle.Core.Logging;
using Castle.DynamicProxy;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace AOPDynamicProxy
{
    internal class DynamicProxyGenerator
    {
        private static DynamicProxyGenerator m_dynamicProxyGenerator = new DynamicProxyGenerator();
        private ProxyGenerator m_proxyGenerator;

        private ILogger m_logger;

        /// <summary>
        /// 标识Logger是否已设置
        /// </summary>
        private bool m_hasLoggerSetted;

        /// <summary>
        /// 私有构造器 不允许外界访问
        /// </summary>
        private DynamicProxyGenerator()
        {
            m_proxyGenerator = new ProxyGenerator();
            m_hasLoggerSetted = false;
        }

        /// <summary>
        /// 返回单例的动态代理生成器
        /// </summary>
        /// <returns></returns>
        internal static DynamicProxyGenerator GetSingleGenerator()
        {
            return m_dynamicProxyGenerator;
        }

        #region 设置Logger 方法
        /// <summary>
        /// 根据Log4net生成ILogger
        /// </summary>
        /// <param name="logger">Log4net.ILog的实现类</param>
        internal void SetLog4netAsILogger(ILog logger, bool isResetLogger = false)
        {
            if (logger == null)
                throw new ArgumentNullException("DynamicProxyGenerator.SetLogger()传入的[logger]不可为null");

            if (m_hasLoggerSetted && !isResetLogger)
                return;
            m_logger = new Logger_log4net(logger);
            m_hasLoggerSetted = true;
        }
        #endregion 设置Logger 方法


        #region 创建动态代理 方法

        #region 1.基于类型创建动态代理

        #region 1.1 withoutTarget
        /// <summary>
        /// 创建[无参构造器]目标类型的动态代理对象 不生成目标对象
        /// </summary>
        /// <typeparam name="TClass">目标类型Type</typeparam>
        /// <param name="customInterceptors">实现了[ICustomInterceptor]接口的自定义拦截器</param>
        /// <returns></returns>
        internal TClass CreateClassProxy<TClass>(Dictionary<MethodInfo, List<EntityInterceptorInfo>> customInterceptors) where TClass : class
        {
            var terminateInterceptor = CreateIInterceptor(customInterceptors);
            try
            {
                return m_proxyGenerator.CreateClassProxy<TClass>(terminateInterceptor);
            }
            catch (Exception ex)
            {
                m_logger?.Error($"DynamicProxyGenerator.CreateClassProxy<TClass>(Dictionary<MethodInfo, List<ICustomInterceptor>> customInterceptors)创建动态代理时异常，ex：{ex.ToString()}");
                throw;
            }
        }

        /// <summary>
        /// 创建[带参构造器]目标类型的动态代理对象 不生成目标对象
        /// </summary>
        /// <typeparam name="TClass">目标类型Type</typeparam>
        /// <param name="constructorArguments">目标类型的构造器参数</param>
        /// <param name="customInterceptors">实现了[ICustomInterceptor]接口的自定义拦截器</param>
        /// <returns></returns>
        internal TClass CreateClassProxy<TClass>(object[] constructorArguments, Dictionary<MethodInfo, List<EntityInterceptorInfo>> customInterceptors) where TClass : class
        {
            Type classToProxy = typeof(TClass);
            var terminateInterceptor = CreateIInterceptor(customInterceptors);
            try
            {
                return m_proxyGenerator.CreateClassProxy(classToProxy, constructorArguments, terminateInterceptor) as TClass;
            }
            catch (Exception ex)
            {
                m_logger?.Error($"DynamicProxyGenerator.CreateClassProxy<TClass>(object[] constructorArguments, Dictionary<MethodInfo, List<ICustomInterceptor>> customInterceptors)创建动态代理时异常，ex：{ex.ToString()}");
                throw;
            }
        }

        /// <summary>
        /// 创建[无参构造器]目标类型的动态代理对象，且目标类型实现了指定接口 不生成目标对象
        /// </summary>
        /// <typeparam name="TClass">目标类型Type</typeparam>
        /// <param name="additionalInterfacesToProxy">目标类型实现的接口</param>
        /// <param name="customInterceptors">实现了[ICustomInterceptor]接口的自定义拦截器</param>
        /// <returns></returns>
        internal TClass CreateClassProxy<TClass>(Type[] additionalInterfacesToProxy, Dictionary<MethodInfo, List<EntityInterceptorInfo>> customInterceptors) where TClass : class
        {
            Type classToProxy = typeof(TClass);
            var terminateInterceptor = CreateIInterceptor(customInterceptors);
            try
            {
                return m_proxyGenerator.CreateClassProxy(classToProxy, additionalInterfacesToProxy, terminateInterceptor) as TClass;
            }
            catch (Exception ex)
            {
                m_logger?.Error($"DynamicProxyGenerator.CreateClassProxy<TClass>(Type[] additionalInterfacesToProxy, Dictionary<MethodInfo, List<ICustomInterceptor>> customInterceptors)创建动态代理时异常，ex：{ex.ToString()}");
                throw;
            }
        }

        /// <summary>
        /// 创建[带参构造器]目标类型的动态代理对象，且目标类型实现了指定接口 不生成目标对象
        /// </summary>
        /// <typeparam name="TClass">目标类型Type</typeparam>
        /// <param name="additionalInterfacesToProxy">目标类型实现的接口</param>
        /// <param name="constructorArguments">目标类型的构造器参数</param>
        /// <param name="customInterceptors">实现了[ICustomInterceptor]接口的自定义拦截器</param>
        /// <returns></returns>
        internal TClass CreateClassProxy<TClass>(Type[] additionalInterfacesToProxy, object[] constructorArguments, Dictionary<MethodInfo, List<EntityInterceptorInfo>> customInterceptors) where TClass : class
        {
            Type classToProxy = typeof(TClass);
            var terminateInterceptor = CreateIInterceptor(customInterceptors);
            try
            {
                return m_proxyGenerator.CreateClassProxy(classToProxy, additionalInterfacesToProxy, new ProxyGenerationOptions(), constructorArguments, terminateInterceptor) as TClass;
            }
            catch (Exception ex)
            {
                m_logger?.Error($"DynamicProxyGenerator.CreateClassProxy<TClass>(Type[] additionalInterfacesToProxy, object[] constructorArguments, Dictionary<MethodInfo, List<ICustomInterceptor>> customInterceptors)创建动态代理时异常，ex：{ex.ToString()}");
                throw;
            }
        }
        #endregion 1.1 withoutTarget

        #region 1.2 withTarget
        /// <summary>
        /// 创建[无参构造器]目标类型的动态代理对象 生成目标对象
        /// </summary>
        /// <typeparam name="TClass">目标类型Type</typeparam>
        /// <param name="target">目标类型对象</param>
        /// <param name="customInterceptors">实现了[ICustomInterceptor]接口的自定义拦截器</param>
        /// <returns></returns>
        internal TClass CreateClassProxyWithTarget<TClass>(TClass target, Dictionary<MethodInfo, List<EntityInterceptorInfo>> customInterceptors) where TClass : class
        {
            var terminateInterceptor = CreateIInterceptor(customInterceptors);
            try
            {
                return m_proxyGenerator.CreateClassProxyWithTarget<TClass>(target, terminateInterceptor);
            }
            catch (Exception ex)
            {
                m_logger?.Error($"DynamicProxyGenerator.CreateClassProxyWithTarget<TClass>(TClass target, Dictionary<MethodInfo, List<ICustomInterceptor>> customInterceptors)创建动态代理时异常，ex：{ex.ToString()}");
                throw;
            }
        }



        /// <summary>
        /// 创建[带参构造器]目标类型的动态代理对象 生成目标对象
        /// </summary>
        /// <typeparam name="TClass">目标类型Type</typeparam>
        /// <param name="target">目标类型对象</param>
        /// <param name="constructorArguments">目标类型的构造器参数</param>
        /// <param name="customInterceptors">实现了[ICustomInterceptor]接口的自定义拦截器</param>
        /// <returns></returns>
        internal TClass CreateClassProxyWithTarget<TClass>(TClass target, object[] constructorArguments, Dictionary<MethodInfo, List<EntityInterceptorInfo>> customInterceptors) where TClass : class
        {
            Type classToProxy = typeof(TClass);
            var terminateInterceptor = CreateIInterceptor(customInterceptors);
            try
            {
                return m_proxyGenerator.CreateClassProxyWithTarget(classToProxy, target, constructorArguments, terminateInterceptor) as TClass;
            }
            catch (Exception ex)
            {
                m_logger?.Error($"DynamicProxyGenerator.CreateClassProxyWithTarget<TClass>(TClass target, object[] constructorArguments, Dictionary<MethodInfo, List<ICustomInterceptor>> customInterceptors)创建动态代理时异常，ex：{ex.ToString()}");
                throw;
            }
        }

        /// <summary>
        /// 创建[无参构造器]目标类型的动态代理对象，且目标类型实现了指定接口 生成目标对象
        /// </summary>
        /// <typeparam name="TClass">目标类型Type</typeparam>
        /// <param name="additionalInterfacesToProxy">目标类型实现的接口</param>
        /// <param name="target">目标类型对象</param>
        /// <param name="customInterceptors">实现了[ICustomInterceptor]接口的自定义拦截器</param>
        /// <returns></returns>
        internal TClass CreateClassProxyWithTarget<TClass>(Type[] additionalInterfacesToProxy, TClass target, Dictionary<MethodInfo, List<EntityInterceptorInfo>> customInterceptors) where TClass : class
        {
            Type classToProxy = typeof(TClass);
            var terminateInterceptor = CreateIInterceptor(customInterceptors);
            try
            {
                return m_proxyGenerator.CreateClassProxyWithTarget(classToProxy, additionalInterfacesToProxy, target, terminateInterceptor) as TClass;
            }
            catch (Exception ex)
            {
                m_logger?.Error($"DynamicProxyGenerator.CreateClassProxyWithTarget<TClass>(Type[] additionalInterfacesToProxy, TClass target, Dictionary<MethodInfo, List<ICustomInterceptor>> customInterceptors)创建动态代理时异常，ex：{ex.ToString()}");
                throw;
            }
        }

        /// <summary>
        /// 创建[带参构造器]目标类型的动态代理对象，且目标类型实现了指定接口 生成目标对象
        /// </summary>
        /// <typeparam name="TClass">目标类型Type</typeparam>
        /// <param name="additionalInterfacesToProxy">目标类型实现的接口</param>
        /// <param name="target">目标类型对象</param>
        /// <param name="constructorArguments">目标类型的构造器参数</param>
        /// <param name="customInterceptors">实现了[ICustomInterceptor]接口的自定义拦截器</param>
        /// <returns></returns>
        internal TClass CreateClassProxyWithTarget<TClass>(Type[] additionalInterfacesToProxy, TClass target, object[] constructorArguments, Dictionary<MethodInfo, List<EntityInterceptorInfo>> customInterceptors) where TClass : class
        {
            Type classToProxy = typeof(TClass);
            var terminateInterceptor = CreateIInterceptor(customInterceptors);
            try
            {
                return m_proxyGenerator.CreateClassProxyWithTarget(classToProxy, additionalInterfacesToProxy, target, new ProxyGenerationOptions(), constructorArguments, terminateInterceptor) as TClass;
            }
            catch (Exception ex)
            {
                m_logger?.Error($"DynamicProxyGenerator.CreateClassProxyWithTarget<TClass>(Type[] additionalInterfacesToProxy, TClass target, object[] constructorArguments, Dictionary<MethodInfo, List<ICustomInterceptor>> customInterceptors)创建动态代理时异常，ex：{ex.ToString()}");
                throw;
            }
        }
        #endregion 1.2 withTarget

        #endregion 1.基于类型创建动态代理



        #region 2.基于接口创建动态代理【无法创建目标类型[只有带参构造器]的接口对象】

        #region 2.1 withoutTarget 【此类方法使用运行时生成的代理接口的空壳实现，故代理方法的实际实现必须由给定的拦截器实现提供【负责设置代理方法的返回值(和输出参数)】。拦截器调用Castle.DynamicProxy.IInvocation.Proceed也是非法的，因为没有实际的实现可以继续】
        /// <summary>
        /// 创建实现了目标接口的动态代理对象 不生成目标对象
        /// </summary>
        /// <typeparam name="TInterface">目标接口类型</typeparam>
        /// <param name="customInterceptor">实现了[ICustomInterceptor]接口的自定义拦截器</param>
        /// <returns>返回目标接口的动态代理实现</returns>
        internal TInterface CreateInterfaceProxyWithoutTarget<TInterface>(Dictionary<MethodInfo, List<EntityInterceptorInfo>> customInterceptor) where TInterface : class
        {
            var terminateInterceptor = CreateIInterceptor(customInterceptor);
            try
            {
                return m_proxyGenerator.CreateInterfaceProxyWithoutTarget<TInterface>(terminateInterceptor);
            }
            catch (Exception ex)
            {
                m_logger?.Error($"DynamicProxyGenerator.CreateInterfaceProxyWithoutTarget<TInterface>(Dictionary<MethodInfo, List<ICustomInterceptor>> customInterceptor)创建动态代理时异常，ex：{ex.ToString()}");
                throw;
            }
        }

        /// <summary>
        /// 创建实现了目标接口&其他指定接口的动态代理对象
        /// </summary>
        /// <typeparam name="TInterface">目标接口类型</typeparam>
        /// <param name="additionalInterfacesToProxy">目标类型实现的其他接口</param>
        /// <param name="customInterceptor">实现了[ICustomInterceptor]接口的自定义拦截器</param>
        /// <returns>返回目标接口的动态代理实现</returns>
        internal TInterface CreateInterfaceProxyWithoutTarget<TInterface>(Type[] additionalInterfacesToProxy, Dictionary<MethodInfo, List<EntityInterceptorInfo>> customInterceptor) where TInterface : class
        {
            var type = typeof(TInterface);
            var terminateInterceptor = CreateIInterceptor(customInterceptor);
            try
            {
                return m_proxyGenerator.CreateInterfaceProxyWithoutTarget(type, additionalInterfacesToProxy, terminateInterceptor) as TInterface;
            }
            catch (Exception ex)
            {
                m_logger?.Error($"DynamicProxyGenerator.CreateInterfaceProxyWithoutTarget<TInterface>(Type[] additionalInterfacesToProxy, Dictionary<MethodInfo, List<ICustomInterceptor>> customInterceptor)创建动态代理时异常，ex：{ex.ToString()}");
                throw;
            }
        }
        #endregion 2.1 withoutTarget


        #region 2.2 withTarget 与[withTargetInterface]创建的代理对象使用无区别，区别在于[Castle.DynamicProxy.ProxyGenerator]中([withTargetInterface]会使用缓存？)
        /// <summary>
        /// 创建实现了目标接口的动态代理对象 生成目标对象
        /// </summary>
        /// <typeparam name="TInterface">目标接口类型</typeparam>
        /// <param name="target">目标类型对象</param>
        /// <param name="customInterceptors">实现了[ICustomInterceptor]接口的自定义拦截器</param>
        /// <returns></returns>
        internal TInterface CreateInterfaceProxyWithTarget<TInterface>(TInterface target, Dictionary<MethodInfo, List<EntityInterceptorInfo>> customInterceptors) where TInterface : class
        {
            var terminateInterceptor = CreateIInterceptor(customInterceptors);
            try
            {
                return m_proxyGenerator.CreateInterfaceProxyWithTarget<TInterface>(target, terminateInterceptor);
            }
            catch (Exception ex)
            {
                m_logger?.Error($"DynamicProxyGenerator.CreateInterfaceProxyWithTarget<TInterface>(TInterface target, Dictionary<MethodInfo, List<ICustomInterceptor>> customInterceptors)创建动态代理时异常，ex：{ex.ToString()}");
                throw;
            }
        }

        /// <summary>
        /// 创建实现了目标接口&其他指定接口的动态代理对象 生成目标对象
        /// </summary>
        /// <typeparam name="TInterface">目标接口类型</typeparam>
        /// <param name="additionalInterfacesToProxy">目标类型实现的其他接口</param>
        /// <param name="target">目标类型对象</param>
        /// <param name="customInterceptors">实现了[ICustomInterceptor]接口的自定义拦截器</param>
        /// <returns></returns>
        internal TInterface CreateInterfaceProxyWithTarget<TInterface>(Type[] additionalInterfacesToProxy, TInterface target, Dictionary<MethodInfo, List<EntityInterceptorInfo>> customInterceptors) where TInterface : class
        {
            var type = typeof(TInterface);
            var terminateInterceptor = CreateIInterceptor(customInterceptors);
            try
            {
                return m_proxyGenerator.CreateInterfaceProxyWithTarget(type, additionalInterfacesToProxy, target, terminateInterceptor) as TInterface;
            }
            catch (Exception ex)
            {
                m_logger?.Error($"DynamicProxyGenerator.CreateInterfaceProxyWithTarget<TInterface>(Type[] additionalInterfacesToProxy, TInterface target, Dictionary<MethodInfo, List<ICustomInterceptor>> customInterceptors)创建动态代理时异常，ex：{ex.ToString()}");
                throw;
            }
        }
        #endregion 2.2 withTarget


        #region 2.3 withTargetInterface 与[withTarget]创建的代理对象使用无区别，区别在于[Castle.DynamicProxy.ProxyGenerator]中([withTargetInterface]会使用缓存？)
        /// <summary>
        /// 创建实现了目标接口的动态代理对象 生成目标对象
        /// </summary>
        /// <typeparam name="TInterface">目标接口类型</typeparam>
        /// <param name="target">目标类型对象</param>
        /// <param name="customInterceptors">实现了[ICustomInterceptor]接口的自定义拦截器</param>
        /// <returns></returns>
        internal TInterface CreateInterfaceProxyWithTargetInterface<TInterface>(TInterface target, Dictionary<MethodInfo, List<EntityInterceptorInfo>> customInterceptors) where TInterface : class
        {
            var terminateInterceptor = CreateIInterceptor(customInterceptors);
            try
            {
                return m_proxyGenerator.CreateInterfaceProxyWithTargetInterface<TInterface>(target, terminateInterceptor);
            }
            catch (Exception ex)
            {
                m_logger?.Error($"DynamicProxyGenerator.CreateInterfaceProxyWithTargetInterface<TInterface>(TInterface target, Dictionary<MethodInfo, List<ICustomInterceptor>> customInterceptors)创建动态代理时异常，ex：{ex.ToString()}");
                throw;
            }
        }

        /// <summary>
        /// 创建实现了目标接口&其他指定接口的动态代理对象 生成目标对象
        /// </summary>
        /// <typeparam name="TInterface">目标接口类型</typeparam>
        /// <param name="additionalInterfacesToProxy">目标类型实现的其他接口</param>
        /// <param name="target">目标类型对象</param>
        /// <param name="customInterceptors">实现了[ICustomInterceptor]接口的自定义拦截器</param>
        /// <returns></returns>
        internal TInterface CreateInterfaceProxyWithTargetInterface<TInterface>(Type[] additionalInterfacesToProxy, TInterface target, Dictionary<MethodInfo, List<EntityInterceptorInfo>> customInterceptors) where TInterface : class
        {
            var type = typeof(TInterface);
            var terminateInterceptor = CreateIInterceptor(customInterceptors);
            try
            {
                return m_proxyGenerator.CreateInterfaceProxyWithTargetInterface(type, additionalInterfacesToProxy, target, terminateInterceptor) as TInterface;
            }
            catch (Exception ex)
            {
                m_logger?.Error($"DynamicProxyGenerator.CreateInterfaceProxyWithTargetInterface<TInterface>(Type[] additionalInterfacesToProxy, TInterface target, Dictionary<MethodInfo, List<ICustomInterceptor>> customInterceptors)创建动态代理时异常，ex：{ex.ToString()}");
                throw;
            }
        }
        #endregion 2.3 withTargetInterface

        #endregion 2.基于接口创建动态代理

        #endregion 创建动态代理 方法






        #region 私有方法
        /// <summary>
        /// 创建实现了[Castle.DynamicProxy.IInterceptor]接口的拦截器
        /// </summary>
        /// <param name="customInterceptors">实现了[ICustomInterceptor]接口的自定义拦截器</param>
        /// <param name="logWriter">日志记录器</param>
        /// <returns></returns>
        private IInterceptor CreateIInterceptor(Dictionary<MethodInfo, List<EntityInterceptorInfo>> customInterceptors)
        {
            try
            {
                return new CastleInterceptor(customInterceptors, m_logger);
            }
            catch (Exception ex)
            {
                m_logger?.Error($"DynamicProxyGenerator.CreateIInterceptor(Dictionary<MethodInfo, List<ICustomInterceptor>> customInterceptors)创建[CastleInterceptor]时异常，ex：{ex.ToString()}");
                throw;
            }
        }
        #endregion 私有方法

    }
}
