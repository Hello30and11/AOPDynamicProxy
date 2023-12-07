using Castle.Core.Logging;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace AOPDynamicProxy
{
    /// <summary>
    /// 动态代理工厂
    /// 用于生成[类型]or[接口]的动态代理对象
    /// </summary>
    public class DynamicProxyFactory
    {
        #region 公共方法

        #region 设置Logger 首次创建代理对象前,须先设置Logger 否则将使用默认的Logger记录日志(可能记录失败)
        /// <summary>
        /// 设置Logger
        /// </summary>
        /// <param name="logger"></param>
        public static void SetLogger<ILogType>(ILogType logger, bool isResetLogger = false) where ILogType : class
        {
            if (logger == null)
                throw new ArgumentNullException("DynamicProxyFactory.SetLogger()传入的logger不可为null");
            if (!(logger is ILog))
                throw new ArgumentException("DynamicProxyFactory.SetLogger()暂不支持log4net.ILog以外的logger");
            //设置Logger
            DynamicProxyGenerator.GetSingleGenerator().SetLog4netAsILogger(logger as ILog, isResetLogger);
        }
        #endregion


        #region 基于类型创建动态代理
        /// <summary>
        /// 创建[无参构造器]目标类型的代理类对象
        /// </summary>
        /// <typeparam name="TClass">目标类型，必须为包含无参构造器的引用类型</typeparam>
        /// <param name="withTargetObj">是否创建目标类型对象 true:目标方法在目标类型对象中执行;false:目标方法在代理对象中执行</param>
        /// <param name="target">目标类型对象</param>
        /// <returns>类型[TClass]的动态代理对象</returns>
        /// <exception cref="System.ArgumentException">当[TClass]不是一个类型时抛出异常</exception>
        public static TClass CreateClassProxy<TClass>(bool withTargetObj = false, TClass target = null) where TClass : class, new() //泛型参数约束:引用类型、无参构造器
        {
            if (!typeof(TClass).IsClass)
                throw new ArgumentException("DynamicProxyFactory.CreateProxy<TClass>()的泛型参数TClass只可为类型");

            Dictionary<MethodInfo, List<EntityInterceptorInfo>> interceptorsInType = new Dictionary<MethodInfo, List<EntityInterceptorInfo>>();
            GetCustomInterceptorsInType(typeof(TClass), ref interceptorsInType);

            if (withTargetObj)
            {
                target = target == null ? new TClass() : target;
                return DynamicProxyGenerator.GetSingleGenerator()
                                            .CreateClassProxyWithTarget<TClass>(target, interceptorsInType);
            }
            else
            {
                return DynamicProxyGenerator.GetSingleGenerator()
                                            .CreateClassProxy<TClass>(interceptorsInType);
            }
        }

        /// <summary>
        /// 创建[无参构造器]目标类型的代理类对象，且目标类型实现了指定接口
        /// </summary>
        /// <typeparam name="TClass">目标类型</typeparam>
        /// <param name="additionalInterfacesToProxy">目标类型实现的接口</param>
        /// <param name="withTargetObj">是否创建目标类型对象 true:目标方法在目标类型对象中执行;false:目标方法在代理对象中执行</param>
        /// <param name="target">目标类型对象</param>
        /// <returns></returns>
        public static TClass CreateClassProxy<TClass>(Type[] additionalInterfacesToProxy, bool withTargetObj = false, TClass target = null) where TClass : class, new()
        {
            if (!typeof(TClass).IsClass)
                throw new ArgumentException("DynamicProxyFactory.CreateProxy<TClass>()的泛型参数TClass只可为类型");

            Dictionary<MethodInfo, List<EntityInterceptorInfo>> interceptorsInType = new Dictionary<MethodInfo, List<EntityInterceptorInfo>>();
            GetCustomInterceptorsInType(typeof(TClass), ref interceptorsInType);
            for (int i = 0; i < additionalInterfacesToProxy.Length; i++)
            {
                var interfaceType = additionalInterfacesToProxy[i];
                GetCustomInterceptorsInType(interfaceType, ref interceptorsInType);
            }

            if (withTargetObj)
            {
                target = target == null ? new TClass() : target;
                return DynamicProxyGenerator.GetSingleGenerator()
                                            .CreateClassProxyWithTarget<TClass>(additionalInterfacesToProxy, target, interceptorsInType);
            }
            else
            {
                return DynamicProxyGenerator.GetSingleGenerator()
                                            .CreateClassProxy<TClass>(additionalInterfacesToProxy, interceptorsInType);
            }
        }

        /// <summary>
        /// 创建[带参构造器]目标类型的代理类对象
        /// </summary>
        /// <typeparam name="TClass">目标类型</typeparam>
        /// <param name="withTargetObj">是否创建目标类型对象 true:目标方法在目标类型对象中执行;false:目标方法在代理对象中执行</param>
        /// <param name="target">目标类型对象</param>
        /// <param name="constructorArguments">目标类型的构造器参数</param>
        /// <returns></returns>
        public static TClass CreateClassProxy<TClass>(object[] constructorArguments, bool withTargetObj = false, TClass target = null) where TClass : class
        {
            if (!typeof(TClass).IsClass)
                throw new ArgumentException("DynamicProxyFactory.CreateProxy<TClass>()的泛型参数TClass只可为类型");

            Dictionary<MethodInfo, List<EntityInterceptorInfo>> interceptorsInType = new Dictionary<MethodInfo, List<EntityInterceptorInfo>>();
            GetCustomInterceptorsInType(typeof(TClass), ref interceptorsInType);

            if (withTargetObj)
            {
                target = target == null ? Activator.CreateInstance(typeof(TClass), constructorArguments) as TClass : target;
                return DynamicProxyGenerator.GetSingleGenerator()
                                            .CreateClassProxyWithTarget<TClass>(target, constructorArguments, interceptorsInType);
            }
            else
            {
                return DynamicProxyGenerator.GetSingleGenerator()
                                           .CreateClassProxy<TClass>(constructorArguments, interceptorsInType);
            }
        }

        /// <summary>
        /// 创建[带参构造器]目标类型的代理类对象，且目标类型实现了指定接口
        /// </summary>
        /// <typeparam name="TClass">目标类型</typeparam>
        /// <param name="additionalInterfacesToProxy">目标类型实现的额外接口</param>
        /// <param name="constructorArguments">目标类型的构造器参数</param>
        /// <param name="withTargetObj">是否创建目标类型对象 true:目标方法在目标类型对象中执行;false:目标方法在代理对象中执行</param>
        /// <param name="target">目标类型对象</param>
        /// <returns></returns>
        public static TClass CreateClassProxy<TClass>(Type[] additionalInterfacesToProxy, object[] constructorArguments, bool withTargetObj = false, TClass target = null) where TClass : class
        {
            if (!typeof(TClass).IsClass)
                throw new ArgumentException("DynamicProxyFactory.CreateProxy<TClass>()的泛型参数TClass只可为类型");

            Dictionary<MethodInfo, List<EntityInterceptorInfo>> interceptorsInType = new Dictionary<MethodInfo, List<EntityInterceptorInfo>>();
            GetCustomInterceptorsInType(typeof(TClass), ref interceptorsInType);
            for (int i = 0; i < additionalInterfacesToProxy.Length; i++)
            {
                var interfaceType = additionalInterfacesToProxy[i];
                GetCustomInterceptorsInType(interfaceType, ref interceptorsInType);
            }

            if (withTargetObj)
            {
                target = target == null ? Activator.CreateInstance(typeof(TClass), constructorArguments) as TClass : target;
                return DynamicProxyGenerator.GetSingleGenerator()
                                            .CreateClassProxyWithTarget<TClass>(additionalInterfacesToProxy, target, constructorArguments, interceptorsInType);
            }
            else
            {
                return DynamicProxyGenerator.GetSingleGenerator()
                                           .CreateClassProxy<TClass>(additionalInterfacesToProxy, constructorArguments, interceptorsInType);
            }
        }
        #endregion 基于类型创建动态代理


        #region 基于接口创建动态代理
        /// <summary>
        /// 创建实现了指定接口的[无参构造器]目标类型代理对象 运行时生成空壳实现，须在拦截器中自行实现接口方法
        /// </summary>
        /// <typeparam name="TInterface">目标接口Type</typeparam>
        /// <returns>返回的代理对象为空壳实现，必须在拦截器中设置返回值ReturnValue</returns>
        public static TInterface CreateInterfaceProxy<TInterface>() where TInterface : class
        {
            if (!typeof(TInterface).IsInterface)
                throw new ArgumentException("DynamicProxyFactory.CreateInterfaceProxy<TInterface>()的泛型参数TInterface只可为接口");

            Dictionary<MethodInfo, List<EntityInterceptorInfo>> interceptorsInType = new Dictionary<MethodInfo, List<EntityInterceptorInfo>>();
            GetCustomInterceptorsInType(typeof(TInterface), ref interceptorsInType);

            return DynamicProxyGenerator.GetSingleGenerator()
                                        .CreateInterfaceProxyWithoutTarget<TInterface>(interceptorsInType);
        }

        /// <summary>
        /// 创建实现了指定接口的[无参构造器]目标类型代理对象 生成目标对象
        /// </summary>
        /// <typeparam name="TInterface">目标接口Type</typeparam>
        /// <typeparam name="TClass">目标类型Type</typeparam>
        /// <param name="target">目标类型对象</param>
        /// <returns></returns>
        public static TInterface CreateInterfaceProxy<TInterface, TClass>(TInterface target = null)
            where TInterface : class
            where TClass : TInterface, new()
        {
            if (!typeof(TInterface).IsInterface)
                throw new ArgumentException("DynamicProxyFactory.CreateInterfaceProxy<TInterface,TClass>()的泛型参数TInterface只可为接口");
            if (!typeof(TClass).IsClass)
                throw new ArgumentException("DynamicProxyFactory.CreateInterfaceProxy<TInterface,TClass>()的泛型参数TClass只可为类型");

            Dictionary<MethodInfo, List<EntityInterceptorInfo>> interceptorsInType = new Dictionary<MethodInfo, List<EntityInterceptorInfo>>();
            GetCustomInterceptorsInType(typeof(TInterface), ref interceptorsInType);
            GetCustomInterceptorsInType(typeof(TClass), ref interceptorsInType);

            target = target == null ? new TClass() : target;
            return DynamicProxyGenerator.GetSingleGenerator()
                                        .CreateInterfaceProxyWithTargetInterface<TInterface>(target, interceptorsInType);
        }

        /// <summary>
        /// 创建实现了指定接口&额外接口的[无参构造器]目标类型代理对象 运行时生成空壳实现，须在拦截器中自行实现接口方法
        /// </summary>
        /// <typeparam name="TInterface">目标接口Type</typeparam>
        /// <param name="additionalInterfacesToProxy">目标类型实现的额外接口</param>
        /// <param name="withTargetObj">是否创建目标类型对象 true:目标方法在目标类型对象中执行;false:目标方法在代理对象中执行</param>
        /// <param name="target">目标类型对象</param>
        /// <returns></returns>
        public static TInterface CreateInterfaceProxy<TInterface>(Type[] additionalInterfacesToProxy) where TInterface : class
        {
            if (!typeof(TInterface).IsInterface)
                throw new ArgumentException("DynamicProxyFactory.CreateInterfaceProxy<TInterface>()的泛型参数TInterface只可为接口");

            Dictionary<MethodInfo, List<EntityInterceptorInfo>> interceptorsInType = new Dictionary<MethodInfo, List<EntityInterceptorInfo>>();
            GetCustomInterceptorsInType(typeof(TInterface), ref interceptorsInType);
            for (int i = 0; i < additionalInterfacesToProxy.Length; i++)
            {
                var interfaceType = additionalInterfacesToProxy[i];
                GetCustomInterceptorsInType(interfaceType, ref interceptorsInType);
            }

            return DynamicProxyGenerator.GetSingleGenerator()
                                        .CreateInterfaceProxyWithoutTarget<TInterface>(additionalInterfacesToProxy, interceptorsInType);
        }

        /// <summary>
        /// 创建实现了指定接口&额外接口的[无参构造器]目标类型代理对象 生成目标对象
        /// </summary>
        /// <typeparam name="TInterface">目标接口Type</typeparam>
        /// <typeparam name="TClass">目标类型Type</typeparam>
        /// <param name="additionalInterfacesToProxy">目标类型实现的额外接口</param>
        /// <param name="target">目标类型对象</param>
        /// <returns></returns>
        public static TInterface CreateInterfaceProxy<TInterface, TClass>(Type[] additionalInterfacesToProxy, TInterface target = null)
            where TInterface : class
            where TClass : TInterface, new()
        {
            if (!typeof(TInterface).IsInterface)
                throw new ArgumentException("DynamicProxyFactory.CreateInterfaceProxy<TInterface,TClass>()的泛型参数TInterface只可为接口");
            if (!typeof(TClass).IsClass)
                throw new ArgumentException("DynamicProxyFactory.CreateInterfaceProxy<TInterface,TClass>()的泛型参数TClass只可为类型");

            Dictionary<MethodInfo, List<EntityInterceptorInfo>> interceptorsInType = new Dictionary<MethodInfo, List<EntityInterceptorInfo>>();
            GetCustomInterceptorsInType(typeof(TInterface), ref interceptorsInType);
            GetCustomInterceptorsInType(typeof(TClass), ref interceptorsInType);
            for (int i = 0; i < additionalInterfacesToProxy.Length; i++)
            {
                var interfaceType = additionalInterfacesToProxy[i];
                GetCustomInterceptorsInType(interfaceType, ref interceptorsInType);
            }

            target = target == null ? new TClass() : target;
            return DynamicProxyGenerator.GetSingleGenerator()
                                        .CreateInterfaceProxyWithTargetInterface<TInterface>(additionalInterfacesToProxy, target, interceptorsInType);
        }
        #endregion 基于接口创建动态代理

        #endregion 公共方法


        #region 私有方法
        /// <summary>
        /// 获取指定Type中 [Type自身]、[Type的公共Method上] 绑定的拦截器(ICustomInterceptor)
        /// </summary>
        /// <param name="type"></param>
        /// <param name="interceptorsOnMethodDic">方法对应的拦截器list 字典</param>
        private static void GetCustomInterceptorsInType(Type type, ref Dictionary<MethodInfo, List<EntityInterceptorInfo>> interceptorsOnMethodDic)
        {
            var customInterceptorsOnTypeSelf = GetCustomInterceptorsOnTypeSelf(type); //获取类型OR接口自身绑定的自定义拦截器

            var methodsInType = GetMethodsInType(type); //获取类型OR接口中定义的方法
            GetCustomInterceptorsDicByMethods(methodsInType, ref interceptorsOnMethodDic); //获取各个方法自身绑定的自定义拦截器

            foreach (var methodInfo in methodsInType)
            {
                List<EntityInterceptorInfo> interceptorsOnMethod;
                bool result = interceptorsOnMethodDic.TryGetValue(methodInfo, out interceptorsOnMethod);
                if (result)
                {
                    interceptorsOnMethod.AddRange(customInterceptorsOnTypeSelf);
                }
                else
                {
                    interceptorsOnMethodDic.Add(methodInfo, customInterceptorsOnTypeSelf);
                }
            }
            return;
        }

        private static MethodInfo[] GetMethodsInType(Type type)
        {
            IEnumerable<MethodInfo> methodInfos;
            if (type.IsClass)
            {
                methodInfos = type.GetMethods(BindingFlags.Public
                                     | BindingFlags.NonPublic
                                     | BindingFlags.Instance
                                     | BindingFlags.Static)
                                  .Where(item => item.IsVirtual);
            }
            else if (type.IsInterface)
            {
                methodInfos = type.GetMethods();
            }
            else
            {
                methodInfos = type.GetMethods();
            }
            //去除object基类定义的方法
            methodInfos = methodInfos.Where(item => !item.DeclaringType.Equals(typeof(object)));
            return methodInfos.ToArray();
        }

        /// <summary>
        /// 获取[类型||接口]本身绑定的自定义拦截器
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private static List<EntityInterceptorInfo> GetCustomInterceptorsOnTypeSelf(Type type)
        {
            List<EntityInterceptorInfo> customInterceptors = new List<EntityInterceptorInfo>();
            if (type == null)
                return customInterceptors;
            if (type.IsDefined(typeof(BindInterceptorAttribute), false))
            {
                var bindCustomInterceptorsAttrObjs = type.GetCustomAttributes(typeof(BindInterceptorAttribute), false);
                for (int j = 0; j < bindCustomInterceptorsAttrObjs.Length; j++)
                {
                    var bindCustomInterceptorsAttr = bindCustomInterceptorsAttrObjs[j] as BindInterceptorAttribute;
                    if (bindCustomInterceptorsAttr == null)
                        continue;
                    var interceptorType = bindCustomInterceptorsAttr.InterceptorType;
                    if (interceptorType == null)
                        continue;
                    var interceptor = CreateInterceptorInstance(interceptorType, bindCustomInterceptorsAttr.InterceptorConstructArgs);
                    if (interceptor == null)
                    {
                        //todo:是否应该抛出异常？
                        continue;
                    }
                    var interceptorWithNo = new EntityInterceptorInfo(bindCustomInterceptorsAttr.SerialNo, interceptor);
                    if (!customInterceptors.Exists(item => item.Equals(interceptorWithNo)))
                        customInterceptors.Add(interceptorWithNo);
                }
            }
            return customInterceptors;
        }

        /// <summary>
        /// 获取给定的方法数组中各个[方法]自身绑定的自定义拦截器
        /// </summary>
        /// <param name="methodInfos"></param>
        /// <param name="interceptorsOnMethodDic"></param>
        /// <returns></returns>
        private static void GetCustomInterceptorsDicByMethods(MethodInfo[] methodInfos, ref Dictionary<MethodInfo, List<EntityInterceptorInfo>> interceptorsOnMethodDic)
        {
            List<EntityInterceptorInfo> customInterceptors;

            if (methodInfos.Length <= 0)
                return;

            for (int i = 0; i < methodInfos.Length; i++)
            {
                var methodInfo = methodInfos[i];

                if (!methodInfo.IsDefined(typeof(BindInterceptorAttribute), false))
                    continue;

                customInterceptors = new List<EntityInterceptorInfo>();

                var bindCustomInterceptorsAttrObjs = methodInfo.GetCustomAttributes(typeof(BindInterceptorAttribute), false);
                for (int j = 0; j < bindCustomInterceptorsAttrObjs.Length; j++)
                {
                    var bindCustomInterceptorsAttr = bindCustomInterceptorsAttrObjs[j] as BindInterceptorAttribute;
                    if (bindCustomInterceptorsAttr == null)
                        continue;
                    var interceptorType = bindCustomInterceptorsAttr.InterceptorType;
                    if (interceptorType == null)
                        continue;
                    var interceptor = CreateInterceptorInstance(interceptorType, bindCustomInterceptorsAttr.InterceptorConstructArgs);
                    if (interceptor == null)
                    {
                        //todo:是否应该抛出异常？
                        continue;
                    }
                    var interceptorWithNo = new EntityInterceptorInfo(bindCustomInterceptorsAttr.SerialNo, interceptor);
                    if (!customInterceptors.Exists(item => item.Equals(interceptorWithNo)))
                        customInterceptors.Add(interceptorWithNo);
                }
                interceptorsOnMethodDic.Add(methodInfo, customInterceptors);
            }
            return;
        }

        private static ICustomInterceptor CreateInterceptorInstance(Type interceptorType, params object[] constructArgs)
        {
            try
            {
                ICustomInterceptor interceptor;
                if (constructArgs == null || constructArgs.Length <= 0)
                    interceptor = Activator.CreateInstance(interceptorType) as ICustomInterceptor;
                else
                    interceptor = Activator.CreateInstance(interceptorType, constructArgs) as ICustomInterceptor;
                return interceptor;
            }
            catch (MissingMethodException ex)
            {
                if (constructArgs != null && constructArgs.Length > 0)
                {
                    StringBuilder builder = new StringBuilder();
                    for (int i = 0; i < constructArgs.Length; i++)
                    {
                        builder.Append($"{constructArgs[i].GetType().ToString()};");
                    }
                    throw new MissingMethodException($"{ex.Message} 请检查:①该类型是否有构造器;②该类型的构造器参数是否依次为:{builder.ToString()}");
                }
                throw;
            }
        }

        #endregion 私有方法
    }
}
