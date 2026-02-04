using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using System;
using global::WiCAM.Pn4000.Contracts.DependencyInjection;
using global::WiCAM.Pn4000.Contracts.Factorys;
using Microsoft.Extensions.DependencyInjection;
using WiCAM.Pn4000.Contracts.DependencyInjection;
using WiCAM.Pn4000.Contracts.Factorys;

namespace WiCAM.Pn4000.DependencyInjection.Implementations;

internal class ModelFactory : IModelFactory, IFactorio, IScopedFactorio
{
    private readonly IServiceProvider _serviceProvider;

    public ModelFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public T Resolve<T>()
    {
        return _serviceProvider.GetService<T>();
    }

    public object? Resolve(Type t)
    {
        return _serviceProvider.GetService(t);
    }

    public T CreateNew<T>(params object[] p)
    {
        return Create<T>(p);
    }

    public bool ResolveAll()
    {
        return true;
    }

    public T Resolve<T>(params object[] parameter)
    {
        return Create<T>(parameter);
    }

    public T Create<T>()
    {
        return ActivatorUtilities.CreateInstance<T>(_serviceProvider, Array.Empty<object>());
    }

    public T Create<T>(params object[] p)
    {
        return ActivatorUtilities.CreateInstance<T>(_serviceProvider, p);
    }
}