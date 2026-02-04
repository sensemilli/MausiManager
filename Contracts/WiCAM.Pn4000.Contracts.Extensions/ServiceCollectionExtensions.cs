using System;
using Castle.DynamicProxy;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using WiCAM.Pn4000.Contracts.Interceptors;

namespace WiCAM.Pn4000.Contracts.Extensions;

public static class ServiceCollectionExtensions
{
	public static void AddTransientIntercepted<I, T>(this IServiceCollection services) where I : class where T : class, I
	{
		services.AddTransientIntercepted<I, T, TelemetryInterceptor>();
	}

	public static void AddSingletonIntercepted<I, T>(this IServiceCollection services) where I : class where T : class, I
	{
		services.AddSingletonIntercepted<I, T, TelemetryInterceptor>();
	}

	public static void AddSingletonInterceptedInterface<I, T>(this IServiceCollection services) where I : class where T : class, IInitializable, I
	{
		services.AddSingletonIntercepted<I, T, InitializeInterceptor<T>>();
	}

	public static void AddSingletonRegisterBoth<I, T>(this IServiceCollection services) where I : class where T : class, I
	{
		services.AddSingleton<T>();
		services.AddSingleton<I, T>((Func<IServiceProvider, T>)((IServiceProvider x) => x.GetService<T>()));
	}

	public static void AddSingletonIntercepted<TInterface, TImplementation, TInterceptor>(this IServiceCollection services) where TInterface : class where TImplementation : class, TInterface where TInterceptor : class, IInterceptor
	{
		services.TryAddSingleton<IProxyGenerator, ProxyGenerator>();
		services.AddSingleton<TImplementation>();
		services.AddTransient<TInterceptor>();
		services.AddSingleton(delegate(IServiceProvider sp)
		{
			IProxyGenerator? service = sp.GetService<IProxyGenerator>();
			TImplementation service2 = sp.GetService<TImplementation>();
			TInterceptor service3 = sp.GetService<TInterceptor>();
			return service.CreateInterfaceProxyWithTarget((TInterface)(object)service2, service3);
		});
	}

	public static void AddTransientIntercepted<TInterface, TImplementation, TInterceptor>(this IServiceCollection services) where TInterface : class where TImplementation : class, TInterface where TInterceptor : class, IInterceptor
	{
		services.TryAddSingleton<IProxyGenerator, ProxyGenerator>();
		services.AddTransient<TImplementation>();
		services.AddTransient<TInterceptor>();
		services.AddTransient(delegate(IServiceProvider sp)
		{
			IProxyGenerator? service = sp.GetService<IProxyGenerator>();
			TImplementation service2 = sp.GetService<TImplementation>();
			TInterceptor service3 = sp.GetService<TInterceptor>();
			return service.CreateInterfaceProxyWithTarget((TInterface)(object)service2, service3);
		});
	}

	public static void AddSingletonIntercepted<TInterface, TInterceptor>(this IServiceCollection services, Func<IServiceProvider, TInterface> constructionDelegate) where TInterface : class where TInterceptor : class, IInterceptor
	{
		services.TryAddSingleton<IProxyGenerator, ProxyGenerator>();
		services.AddTransient<TInterceptor>();
		services.AddSingleton(delegate(IServiceProvider sp)
		{
			IProxyGenerator? service = sp.GetService<IProxyGenerator>();
			TInterface target = constructionDelegate(sp);
			TInterceptor service2 = sp.GetService<TInterceptor>();
			return service.CreateInterfaceProxyWithTarget(target, service2);
		});
	}
}
