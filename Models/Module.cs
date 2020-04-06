using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ModulaIOT.Device.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Configuration;

namespace ModulaIOT.Device.Models
{
    public interface IModuleLifetime
    {
        TModule Get<TModule>(string id) where TModule : IModule;
    }

    public interface IModule
    {
        string Id { get; }
        string Name { get; }
        Task Run();
    }

    public class ModuleLifetime : IModuleLifetime
    {
        private readonly Dictionary<string, IModule> _modules;
        private readonly IServiceProvider _provider;


        public ModuleLifetime(IServiceProvider provider)
        {
            _modules = new Dictionary<string, IModule>();
            _provider = provider;
        }

        public TModule Get<TModule>(string id) where TModule : IModule
        {
            if (!_modules.ContainsKey(id)) return CreateModule<TModule>(id);
            return (TModule)_modules[id];
        }

        private TModule CreateModule<TModule>(string id) where TModule : IModule
        {
            var instance = ActivatorUtilities.CreateInstance<TModule>(_provider, new string[] { id });
            _modules[id] = instance;
            return instance;
        }
    }

    public abstract class Module : IModule
    {
        protected readonly IConfiguration _config;
        protected readonly IConfiguration _section;

        public string Id { get; }
        public string Name => _section["name"];

        public Module(string id, IConfiguration config)
        {
            _config = config;
            _section = _config.GetModuleConfig(id);
            Id = id;
        }

        public virtual Task Run()
        {
            return Task.CompletedTask;
        }
    }
}

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ModuleExtensions
    {
        public static IServiceCollection AddModuleLifetime(this IServiceCollection services)
        {
            services.AddSingleton<IModuleLifetime, ModuleLifetime>();
            return services;
        }
        public static IServiceCollection AddModule<TService, TImplementation>(this IServiceCollection services, string id)
            where TService : class, IModule
            where TImplementation : class, TService
        {
            services.AddSingleton<TService, TImplementation>(provider =>
            {
                var lifetime = provider.GetService<IModuleLifetime>();
                return lifetime.Get<TImplementation>(id);
            });
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