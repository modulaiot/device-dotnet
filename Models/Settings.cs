using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;


namespace ModulaIOT.Device.Models
{
    public interface ISettings : IDictionary<string, IModule>
    {
        void Load();
        void Save();
        T Get<T>(string id) where T : IModule;
    }

    public class Settings : Dictionary<string, IModule>, ISettings
    {
        private IConfiguration Config;
        private IServiceProvider Provider { get; }

        public Settings(IConfiguration config, IServiceProvider provider)
        {
            Config = config;
            Provider = provider;
        }


        public void Load()
        {
            foreach (var section in Config.GetChildren())
            {
                var typeName = section["Type"];
                try
                {
                    var instance = Provider.GetModule(typeName);
                    section.Bind((object)instance);
                    instance.Id = section.Key;
                    this.Add(instance.Id, instance);
                }
                catch (InvalidOperationException error)
                {
                    Console.WriteLine($"Error: {error.Message}");
                }
            }
        }

        public void Save()
        {

        }

        public T Get<T>(string id) where T : IModule
        {
            return (T)this[id];
        }
    }
}