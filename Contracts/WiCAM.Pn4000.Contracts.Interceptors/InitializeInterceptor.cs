using Castle.DynamicProxy;

namespace WiCAM.Pn4000.Contracts.Interceptors;

public class InitializeInterceptor<T> : IInterceptor where T : IInitializable
{
	private bool _init;

	public void Intercept(IInvocation invocation)
	{
		if (!_init)
		{
			lock (this)
			{
				if (!_init)
				{
					((T)invocation.InvocationTarget).Initialize();
					_init = true;
				}
			}
		}
		invocation.Proceed();
	}
}
