using System;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;


namespace ModulaIOT.Device.Models
{
    public interface ISettings : IDictionary<string, IModuleSettings>, ILoadable
    {
    }

    public interface IModuleSettings : IDictionary<string, object>, ILoadable
    {
    }

    public class Settings : Dictionary<string, IModuleSettings>, ISettings
    {
        private readonly IServiceProvider _provider;
        private readonly IDeviceConfiguration _deviceConfiguration;

        public Settings(IServiceProvider provider, IDeviceConfiguration deviceConfiguration)
        {
            _provider = provider;
            _deviceConfiguration = deviceConfiguration;
        }

        public async Task Load()
        {
            if (!File.Exists(_deviceConfiguration.SettingsPath)) await Save();
            using var fs = File.OpenRead(_deviceConfiguration.SettingsPath);
            var json = await JsonDocument.ParseAsync(fs);
            foreach (var section in json.RootElement.EnumerateObject())
            {
                var settings = ModuleSettings.Parse(section.Value);
                await settings.Load();
                this.Add(section.Name, settings);
            }
        }

        public async Task Save()
        {
            foreach (var settings in Values)
            {
                await settings.Save();
            }

            using var fs = File.Open(_deviceConfiguration.SettingsPath, FileMode.Create);
            var json = this.ToDictionary(x => x.Key, x => x.Value as object);
            var options = new JsonSerializerOptions();
            options.WriteIndented = true;
            await JsonSerializer.SerializeAsync<Dictionary<string, object>>(fs, json, options);
        }
    }

    public class ModuleSettings : Dictionary<string, object>, IModuleSettings
    {

        public ModuleSettings()
        {

        }

        public ModuleSettings(Dictionary<string, object> dictionary) : base(dictionary)
        {

        }

        public Task Load()
        {
            return Task.CompletedTask;
        }

        public Task Save()
        {
            return Task.CompletedTask;
        }

        public static IModuleSettings Parse(JsonElement element)
        {
            return new ModuleSettings(ParseJson(element) as Dictionary<string, object>);
        }

        private static object ParseJson(JsonElement element)
        {
            switch (element.ValueKind)
            {
                case JsonValueKind.Number:
                    return element.GetInt32();
                case JsonValueKind.True:
                case JsonValueKind.False:
                    return element.GetBoolean();
                case JsonValueKind.String:
                    return element.GetString();
                case JsonValueKind.Array:
                    var array = new List<object>();
                    foreach (var item in element.EnumerateArray())
                    {
                        array.Add(ParseJson(item));
                    }
                    return array;
                case JsonValueKind.Object:
                    var obj = new Dictionary<string, object>();
                    foreach (var prop in element.EnumerateObject())
                    {
                        obj.Add(prop.Name, ParseJson(prop.Value));
                    }
                    return obj;
                default:
                    return null;
            }
        }
    }
}