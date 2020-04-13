using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using ModulaIOT.Device.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Configuration;

namespace ModulaIOT.Device.Models
{
    public interface IModuleLifetime
    {
        Task Run();
        TModule GetModule<TModule>() where TModule : IModule;
        IEnumerable<IModule> ListModules();
    }

    public interface IModule
    {
        string Id { get; }
        string Name { get; }
        bool InUse { get; }
        void Use(IModule? module = null);
        void Release(IModule? module = null);
        Task Run(IModuleLifetime lifetime);
    }

    public class ModuleLifetime : IModuleLifetime
    {
        private readonly IServiceCollection _services;
        private readonly IServiceProvider _provider;


        public ModuleLifetime(IServiceCollection services, IServiceProvider provider)
        {
            _services = services;
            _provider = provider;
        }

        public async Task Run()
        {
            var tasks = ListModules().Select(x => x.Run(this));
            await Task.WhenAll(tasks);
        }

        public TModule GetModule<TModule>() where TModule : IModule
        {
            return _provider.GetService<TModule>();
        }

        public IEnumerable<IModule> ListModules()
        {
            return _services
                .Where(x => typeof(IModule).IsAssignableFrom(x.ServiceType))
                .Select(x => (IModule)_provider.GetService(x.ServiceType))
                .Where(x => x != null);
        }


    }

    public abstract class Module : IModule
    {
        protected int _using;
        protected readonly IConfiguration _config;
        protected readonly IConfiguration _section;

        public string Id { get; }
        public string Name => _section["name"];
        public bool InUse => _using > 0;

        public Module(string id, IConfiguration config)
        {
            _using = 0;
            _config = config;
            _section = _config.GetModuleConfig(id);
            Id = id;
        }

        public virtual Task Run(IModuleLifetime lifetime)
        {
            return Task.CompletedTask;
        }

        public void Use(IModule? module = null)
        {
            if (module != null) module.Use();
            else _using += 1;
        }

        public void Release(IModule? module = null)
        {
            if (module != null) module.Release();
            else
            {
                _using -= 1;
                if (_using < 0) throw new Exception($"Unbalanced Using of {Id} module.");
            }
        }

        protected Task WaitUntil(Func<bool> func, int delay = 100)
        {
            return Task.Run(async () =>
            {
                while (!func()) await Task.Delay(delay);
            });
        }

        protected Task WaitUntilUnused()
        {
            return WaitUntil(() => !InUse);
        }
    }

    public class UsingModule : IDisposable
    {
        private readonly IModule _module;

        public UsingModule(IModule module)
        {
            _module = module;
            _module.Use();
        }
        public void Dispose()
        {
            _module.Release();
        }
    }
}

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ModuleExtensions
    {
        public static IServiceCollection AddModuleLifetime(this IServiceCollection services)
        {
            services.AddSingleton<IServiceCollection>(services);
            services.AddSingleton<IModuleLifetime, ModuleLifetime>();
            return services;
        }
        public static IServiceCollection AddModule<TService, TImplementation>(this IServiceCollection services, string id)
            where TService : class, IModule
            where TImplementation : class, TService
        {
            services.AddSingleton<TService, TImplementation>(provider =>
                ActivatorUtilities.CreateInstance<TImplementation>(provider, new string[] { id })
            );
            return services;
        }
        // public static IServiceCollection AddModuleConfig<TService, TImplementation>(this IServiceCollection services)
        //     where TService : class, IModuleConfiguration
        //     where TImplementation : class, TService
        // {
        //     services.AddScoped<TService, TImplementation>();
        //     return services;
        // }
    }
}