using Microsoft.Extensions.DependencyInjection;
using System;
using WiCAM.Pn4000.Contracts.DependencyInjection;

namespace WiCAM.Pn4000.JobManager
{
    internal class Globals : IFactorio
    {
        private readonly IServiceProvider _serviceProvider;

        public Globals(IServiceProvider serviceProvider)
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
}