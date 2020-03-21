using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ModulaIOT.Device.Models;

namespace ModulaIOT.Device.Models
{
    public interface IModule
    {

        string Id { get; set; }
        string Name { get; set; }
        Task Run();
    }
    public abstract class Module : IModule
    {
        public string Id { get; set; }
        public string Name { get; set; }

        public abstract Task Run();
    }
}

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ModuleExtensions
    {

        private static readonly Dictionary<string, Type> ModuleMap = new Dictionary<string, Type>();

        public static IServiceCollection UseModule<TService>(this IServiceCollection services, bool singleton = false)
            where TService : class, IModule
        {
            var type = typeof(TService);
            if (ModuleMap.ContainsKey(type.Name)) throw new InvalidOperationException($"Module {type.Name} already registered.");

            ModuleMap[type.Name] = type;
            if (singleton)
            {
                services.AddSingleton<IModule, TService>();
                services.AddSingleton<TService>();
            }
            else
            {
                services.AddTransient<IModule, TService>();
                services.AddTransient<TService>();
            }

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