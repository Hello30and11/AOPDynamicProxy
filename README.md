# AOPDynamicProxy
在Castle.Core.dll的DynamicProxy基础上进行封装的、运行时创建动态代理对象的AOP框架

使用说明：
①DynamicProxyFactory类为此框架的对外API，其中有2类创建动态代理对象的方法：CreateClassProxy()与CreateInterfaceProxy()，分别基于类型和接口创建代理对象；
②在调用API创建代理对象前，请先使用DynamicProxyFactory.SetLogger<ILogType>()方法设置记录日志的方式(暂时只支持传入log4net.ILog对象)；
③以CreateClassProxy()与CreateInterfaceProxy()创建代理对象后：
  (1)若目标方法绑定了[HandleException]、[NoteElapsedTime]、[WriteLog] Attribute，则目标方法在运行时可相应处理异常、记录耗时、记录日志；
  (2)若目标接口、目标类型或目标方法绑定[BindInterceptor] Attribute传入了自定义拦截器，则目标方法在运行时将在适当时机调用指定的自定义拦截器；
    [BindInterceptor]除了可指定拦截器外，还可传入此拦截器的构造器参数【常量、typeof表达式】，也可指定此拦截器的调用顺序序号【最大可为255，不指定则为255】
  (3)定义自定义拦截器的方式为：实现接口[IBeforeInterceptor]或接口[IAfterInterceptor]。
    实现[IBeforeInterceptor]的拦截器会在目标方法调用之前被调用，实现[IAfterInterceptor]接口的拦截器会在目标方法之后被调用。
④使用此程序集的模块可在目标方法上绑定自定义Attribute(自定义的Attribute继承自[AOPDynamicProxy.CustomAttribute]即可)，之后可在[IBeforeInterceptor]拦截器 和 IAfterInterceptor]拦截器的实现类对象中使用该自定义Attribute；

注意事项：
  ①作为目标对象的[类型]或[接口]必须声明为【public】或【internal,并将目标程序集标记为:[assembly:InternalsVisibleTo("DynamicProxyGenAssembly2")]】；
  ②只有声明为[public]的方法才会作为目标方法，能使用以上描述的全部特性；
  ③定义自定义拦截器时不要实现接口[ICustomInterceptor]，而应实现[IBeforeInterceptor]或[IAfterInterceptor]接口；
