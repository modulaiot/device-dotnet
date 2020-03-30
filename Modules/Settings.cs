using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text.Json;
using System.Runtime.CompilerServices;
using ModulaIOT.Device.Models;


namespace ModulaIOT.Device.Modules.Settings
{
    public interface ISettings
    {
        IEnumerable<string> Keys();
        IEnumerable<string> Values();
        T Get<T>([CallerMemberName] string? prop = null) where T : struct;
        T GetWithDefault<T>(T defaultVal, [CallerMemberName] string? prop = null) where T : class;
        T? GetOrDefault<T>([CallerMemberName] string? prop = null) where T : class;
        void Set<T>(T value, [CallerMemberName] string? prop = null);
    }
}

namespace ModulaIOT.Device.Modules
{
    public interface IFileSettings : IModule
    {
        IEnumerable<string> Keys();
        T Get<T>(string prop) where T : class, new();
        void Set<T>(T? value, string prop) where T : class;
    }

    public interface ISettings : IModule
    {
        T Get<T>([CallerMemberName] string? prop = null) where T : struct;
        T GetWithDefault<T>(T defaultVal, [CallerMemberName] string? prop = null) where T : class;
        T? GetOrDefault<T>([CallerMemberName] string? prop = null) where T : class;
        void Set<T>(T value, [CallerMemberName] string? prop = null);
    }

    public class FileSettings : Module, IFileSettings
    {
        private readonly Dictionary<string, object?> _settings;
        private readonly IConfiguration _deviceConfiguration;

        public FileSettings(IModuleProvider provider, string id) : base(provider, id, true)
        {
            _settings = new Dictionary<string, object?>();
            _deviceConfiguration = _provider.Get<IConfiguration>("Configuration");
        }

        protected override async Task OnUse()
        {
            await _deviceConfiguration.Use();

            if (_settings.Count > 0) return;
            if (!File.Exists(_deviceConfiguration.SettingsPath)) return;

            using var fs = File.OpenRead(_deviceConfiguration.SettingsPath);
            var json = await JsonDocument.ParseAsync(fs);
            foreach (var section in json.RootElement.EnumerateObject())
            {
                _settings.Add(section.Name, ParseJson(section.Value));
            }
        }

        protected override async Task OnRelease()
        {
            using var fs = File.Open(_deviceConfiguration.SettingsPath, FileMode.Create);
            var options = new JsonSerializerOptions();
            options.WriteIndented = true;
            await JsonSerializer.SerializeAsync<Dictionary<string, object?>>(fs, _settings, options);

            await _deviceConfiguration.Release();
        }

        public IEnumerable<string> Keys()
        {
            return _settings.Keys;
        }

        public T Get<T>(string prop) where T : class, new()
        {
            var val = _settings.GetValueOrDefault(prop) as T;
            if (val == null)
            {
                val = new T();
                _settings[prop] = val;
            }
            return val;
        }

        public void Set<T>(T? value, string prop) where T : class
        {
            _settings[prop] = value;
        }

        private static object? ParseJson(JsonElement element)
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
                    var array = new List<object?>();
                    foreach (var item in element.EnumerateArray())
                    {
                        array.Add(ParseJson(item));
                    }
                    return array;
                case JsonValueKind.Object:
                    var obj = new Dictionary<string, object?>();
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

    public class ModuleSettings : Module, ISettings
    {
        protected readonly Dictionary<string, object?> _settings;

        public ModuleSettings(IModuleProvider provider, string id) : base(provider, id, false)
        {
            _settings = _provider.Get<IFileSettings>("FileSettings")
                .Get<Dictionary<string, object?>>(this.Id);
        }

        public T Get<T>([CallerMemberName] string? prop = null) where T : struct
        {
            if (prop == null) throw new Exception("Prop can not be null.");
            var value = GetOrDefault<object>(prop);
            if (value == null) return default;
            return (T)value;
        }

        public T GetWithDefault<T>(T defaultVal, [CallerMemberName] string? prop = null) where T : class
        {
            return GetOrDefault<T>(prop) ?? defaultVal;
        }

        public T? GetOrDefault<T>([CallerMemberName] string? prop = null) where T : class
        {
            if (prop == null) throw new Exception("Prop can not be null.");
            return _settings.GetValueOrDefault(prop) as T ?? default;
        }

        public void Set<T>(T value, [CallerMemberName] string? prop = null)
        {
            if (prop == null) throw new Exception("Prop can not be null.");
            _settings[prop] = value;
        }

        protected override async Task OnUse()
        {
            await _provider.Get("FileSettings").Use();
        }

        protected override async Task OnRelease()
        {
            await _provider.Get("FileSettings").Release();
        }
    }
}