using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ModulaIOT.Device.Models
{

    public interface IModules : IDictionary<string, IModule>, ILoadable
    {
    }

    public interface IModule : ILoadable
    {
        IModuleSettings Settings { get; set; }
        string Id { get; set; }
        string Name { get; set; }
    }
    public class Modules : Dictionary<string, IModule>, IModules
    {
        private readonly ILogger _logger;
        private readonly IServiceProvider _provider;
        private readonly ISettings _settings;

        public Modules(ILogger<Modules> logger, IServiceProvider provider, ISettings settings)
        {
            _logger = logger;
            _provider = provider;
            _settings = settings;
        }

        public async Task Load()
        {
            await _settings.Load();
            foreach (var item in _settings)
            {
                var id = item.Key;
                var settings = item.Value;

                if (!settings.ContainsKey("Type"))
                {
                    _logger.LogWarning($"Settings is missing \"Type\" for Module \"{id}\"");
                    continue;
                }

                var typeName = settings["Type"] as string;
                var instance = _provider.GetModule(typeName);
                instance.Settings = settings;
                instance.Id = item.Key;
                await instance.Load();
                this.Add(instance.Id, instance);
            }
        }

        public async Task Save()
        {
            foreach (var module in Values)
            {
                if (!module.Settings.ContainsKey("Type")) module.Settings["Type"] = module.GetType().Name;
                await module.Save();
            }
            await _settings.Save();
        }
    }
    public abstract class Module : IModule
    {
        public IModuleSettings Settings { get; set; }
        public string Id { get; set; }
        public string Name { get => Settings["Name"] as string; set => Settings["Name"] = value; }

        public virtual Task Load()
        {
            return Task.CompletedTask;
        }

        public virtual Task Save()
        {
            return Task.CompletedTask;
        }
    }

    public static class ModuleExtensions
    {

        private static readonly Dictionary<string, Type> ModuleMap = new Dictionary<string, Type>();

        public static IServiceCollection UseModule<TService>(this IServiceCollection services, bool singleton = false, string typeName = null)
            where TService : class, IModule
        {
            return services.UseModule<TService, TService>(singleton, typeName);
        }
        public static IServiceCollection UseModule<TService, TImplementation>(this IServiceCollection services, bool singleton = false, string typeName = null)
            where TService : class, IModule
            where TImplementation : class, TService
        {
            if (typeName == null) typeName = typeof(TImplementation).Name;
            var type = typeof(TService);
            if (ModuleMap.ContainsKey(typeName)) throw new InvalidOperationException($"Module {typeName} already registered.");

            ModuleMap[typeName] = type;
            if (singleton) services.AddSingleton<TService, TImplementation>();
            else services.AddTransient<TService, TImplementation>();

            return services;
        }

        public static IModule GetModule(this IServiceProvider provider, string typeName)
        {
            if (!ModuleMap.ContainsKey(typeName)) throw new InvalidOperationException($"Module {typeName} is not registered.");

            var type = ModuleMap[typeName];
            var instance = provider.GetService(type);

            return (IModule)instance;
        }



    }
}